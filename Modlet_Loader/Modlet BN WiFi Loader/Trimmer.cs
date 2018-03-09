//#define GPIB_HW_SUPPORT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if GPIB_HW_SUPPORT
using NationalInstruments.NI4882;
#endif

namespace ThinkEco
{
    class Trimmer
    {
        // Trimming target frequency 12MHz
        const double targetFrequency = 12e6;
        // Target tolerance for trimming 
        const double trimTolerancePPM = 2;
        // Crystal parameter d(error[ppm])/d(capacitance[pF])
        const double xtal_de_by_dC = -10.2;
        // Threshold for allowing coarse trim adjustment 
        const double coarseThreshold = 10;
        // Limit # of trimming iterations 
        const int maxTrimIterations = 5;

        FreescaleInterface freescaleInterface;
#if GPIB_HW_SUPPORT
        Device freqCounter;
#endif
        public Trimmer(FreescaleInterface i)
        {
#if GPIB_HW_SUPPORT
            if (Settings.trimCrystal == true)
            {
                // Use supplied SSL interface 
                freescaleInterface = i;

                // Instantiate frequency counter device (make sure counter addresses match settings file values) 
                freqCounter = new Device(Settings.gpibInterfaceID, Settings.freqCounterPrimaryAddr, Settings.freqCounterSecondaryAddr);

                // Check status of device 
                if (0 == freqCounter.GetCurrentStatus()) throw new Exception_STOP("Connection issue with frequency counter");

                // Get counter ID string 
                freqCounter.Write("*IDN?");
                Parameters.freqCounterId = freqCounter.ReadString().Replace(',', ';');

                // Put counter in GPIB mode 
                freqCounter.Write("*RST");
            }
#endif
        }

        public void Run()
        {
            // Initialize count 
            int trimIterations = 0;

            // Set starting values (finetrim should be centered (=15))
            int coarse = Settings.coarseTrimDefaultVal;
            int fine = Settings.fineTrimDefaultVal;

            // Convert trim values to load capacitance 
            double capacitance = (double)coarse + (5.0 / 32.0) * (double)fine;

            // Take frequency measurement at this trim setting 
            double freq = FreqAtTrim(coarse, fine);

            // Calculate frequency error in ppm 
            double ppm = 1e6 * (freq - targetFrequency) / targetFrequency;

            // Iterate until we reach target accuracy 
            while (Math.Abs(ppm) > trimTolerancePPM)
            {
                // Limit number of trimming steps 
                if (++trimIterations > maxTrimIterations) throw new Exception_FAIL("Exceeded allowed iterations for crystal trimming");

                // Adjust capacitance using Newton's method 
                capacitance = capacitance - ppm / xtal_de_by_dC;

                // Do this only for large error 
                if (Math.Abs(ppm) > coarseThreshold)
                {
                    // Find coarse trim value for which fine trim is most centered
                    coarse = (int)Math.Round(capacitance - (5.0 / 32.0) * 15.0);
                }

                // Find fine trim value that best approximates target capacitance 
                fine = (int)Math.Round((32.0 / 5.0) * (capacitance - (double)coarse));

                // Take frequency measurement at this trim setting 
                freq = FreqAtTrim(coarse, fine);

                // Calculate frequency error in ppm 
                ppm = 1e6 * (freq - targetFrequency) / targetFrequency;
            }

            // These are the trim results 
            Parameters.coarseTrim = coarse;
            Parameters.fineTrim = fine;
            Parameters.freqError = (int)Math.Round(ppm);
        }

        private double FreqAtTrim(int coarse, int fine)
        {
            double freq = targetFrequency; 

#if GPIB_HW_SUPPORT
            if (Settings.trimCrystal == true)
            {
                // Send command to set new trim values 
                freescaleInterface.SetXtalTrim(coarse, fine); 

                // Take frequency measurement 
                freqCounter.Write("MEAS:FREQ? 1.2e7,1.2");
                freq = Convert.ToDouble(freqCounter.ReadString());
            }
#endif
            return freq;
        }
    }
}
