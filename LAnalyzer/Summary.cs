using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalActionAnalyzer
{
    public class Summary
    {
        public int nrOfSa;
        public int nrOfSsa;
        public int nrOfQssa;
        public int nrOfDictatorShip;
        public int nrOfDtfs;
        public int nrOfTypeConversionDtfs;
        public int nrOfDecisions;
        public int nrOfBranches;
        public int nrOfLoops;
        public int nrOfPps;
        public int nrOfCps;
        public int nrOfPpFunctions;
        public int nrOfCpFunctions;


        public string GetSummary()
        {
            int total =
                nrOfSa +
                nrOfSsa +
                nrOfQssa +
                nrOfDictatorShip +
                nrOfDtfs +
                nrOfTypeConversionDtfs +
                nrOfDecisions +
                nrOfBranches +
                nrOfLoops +
                nrOfPps +
                nrOfCps +
                nrOfPpFunctions +
                nrOfCpFunctions;

            String summary =
                String.Format("#SAs            : {0,3}\n", nrOfSa) +
                String.Format("#SSAs           : {0,3}\n", nrOfSsa) +
                String.Format("#QSSAs          : {0,3}\n", nrOfQssa) +
                String.Format("#Dictator ships : {0,3}\n", nrOfDictatorShip) +
                String.Format("#DTFs           : {0,3}\n", nrOfDtfs) +
                String.Format("#Type Conv. DTFs: {0,3}\n", nrOfTypeConversionDtfs) +
                String.Format("#Decisions      : {0,3}\n", nrOfDecisions) +
                String.Format("#Branches       : {0,3}\n", nrOfBranches) +
                String.Format("#Loops          : {0,3}\n", nrOfLoops) +
                String.Format("#PP calls       : {0,3}\n", nrOfPps) +
                String.Format("#CP calls       : {0,3}\n", nrOfCps) +
                String.Format("#PP functions   : {0,3}\n", nrOfPpFunctions) +
                String.Format("#CP functions   : {0,3}\n", nrOfCpFunctions) +
                              "---------------------\n" +
                String.Format("#Total items    : {0,3}\n", total);

            return summary;
        }
    }
}
