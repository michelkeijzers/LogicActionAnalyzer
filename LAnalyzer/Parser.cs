using LogicActionAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicAnalyzer
{
    public class Parser
    {
        Summary _summary;
        StringBuilder _output;

        string _compName;
        int _indent;
        string _functionName; // Stored until a { is found, else ignored (it was a prototype)
        int _lineNumber;

        bool _showLoopDetails;


        const int IndentSize = 2;


        public Parser(bool showLoopDetails)
        {
            _showLoopDetails = showLoopDetails;
        }


        public string Parse(string[] la)
        {
            _summary = new Summary();

            _output = new StringBuilder();

            _indent = 0;
            _compName = "";
            _indent = 0;
            _functionName = "";

            string previousLine = "";
            bool appendToNextLine = false;

            _lineNumber = 0;

            foreach (var currentLine in la)
            {
                string line = currentLine;

                if (appendToNextLine)
                {
                    line = previousLine + " " + currentLine;
                }

                if (currentLine.EndsWith("=") || currentLine.EndsWith(","))
                {
                    appendToNextLine = true;
                }
                else
                {
                    appendToNextLine = false;

                    if (line.StartsWith("#include ") && line.Contains("_LA_"))
                    {
                        PrintCompAndLaName(line);
                    }
                    else if (line.Contains("const int entry_id"))
                    {
                        StoreFunctionName(line);
                    }
                    else if (line.Contains("{") && !_functionName.Equals(""))
                    {
                        PrintPpCpFunctionName(line);
                    }
                    else if (line.Contains("_pl("))
                    {
                        PrintPl();
                    }
                    else if (line.Contains("_cl("))
                    {
                        PrintCl();
                    }
                    else if (line.Contains("switch"))
                    {
                        PrintDecisionName(line);
                    }
                    else if (line.Contains("case ("))
                    {
                        PrintDecisionBranchName(line);
                    }
                    else if (line.Contains("default:"))
                    {
                        DecreaseIndent(); // End of branch of a decision
                    }
                    else if (line.Contains(".scan_timing.prep_time ="))
                    {
                        PrintActionName(line);
                    }
                    else if (line.Contains("temp_physical_params_p->wait_for_result = TRUE;"))
                    {
                        PrintWaitForResultTrue();
                    }
                    else if (line.Contains(".scan_type = SNXA_"))
                    {
                        PrintScanType(line);
                    }
                    else if (line.Contains("pp_function_ptr ="))
                    {
                        PrintPpName(line);
                    }
                    else if (line.Contains("cp_function_ptr ="))
                    {
                        PrintCpName(line);
                    }
                    else if (line.Contains("  result =") &&
                             line.Contains("DTF_") &&
                             !line.Contains($"{_compName}XS_DB_") &&
                             !line.Contains($"{_compName}XS_") &&
                             !line.Contains("  result = DDXAx"))
                    {
                        PrintDtfName(line);
                    }
                    else if (line.Contains("for (; "))
                    {
                        PrintLoopName(line);
                    }
                    else if (line.Contains(">= (logical_action_p->nr_iterations_"))
                    {
                        PrintInterruptableLoop(line);
                    }
                    else if (line.Contains("iLOOP_SKIP_WHOLE_LOOP)"))
                    {
                        PrintSkipWholeLoop();
                    }
                    else if (line.Contains("XS_PAS_set_next_action"))
                    {
                        PrintSetNextAction(line);
                    }
                    else if (line.Contains("iLOOP_SKIP_NOTHING"))
                    {
                        PrintSkipNothing();
                    }
                    else if (line.Contains("== logical_action_p->nr_iterations_ExpansionRegion"))
                    {
                        PrintLoopInterruptConditionCheck(line);
                    }
                    else if (line.Contains("XS_PAS_skip_actions"))
                    {
                        PrintSkipActions();
                    }
                    else if (line.Contains("&& (logical_action_p->nr_iterations_ExpansionRegion"))
                    {
                        PrintLoopIterations0(line);
                    }
                    else if (line.Contains("result = ExpansionRegion_") && line.Contains("_ppcb(entry_id"))
                    {
                        PrintExpansionRegionPpcb(line);
                    }
                    else if (line.Contains(" = nr_iterations;"))
                    {
                        DecreaseIndent();
                    }
                }

                previousLine = line;
                _lineNumber++;
            }


            if (_lineNumber > 1) // In case of empty input: empty string is only string
            {
                _output.AppendLine();
                _output.AppendLine(_summary.GetSummary());
            }

            return _output.ToString();
        }


        private void PrintCompAndLaName(string line)
        {
            string[] separatorsComp = { "#include \"", "_LA_" };
            _compName = Split(separatorsComp, line)[0];
            AppendIndentedLine("Component Name: " + _compName);

            string[] separatorsLa = { "_LA_", ".h" };
            AppendIndentedLine("Logical Action Name: " + Split(separatorsLa, line)[1]);
        }


        private void StoreFunctionName(string line)
        {
            string[] separators = { "static int ", "(const int entry_id " };
            _functionName = Split(separators, line)[0];
        }


        private void PrintPpCpFunctionName(string line)
        {
            _indent = 0;
            bool isPp = _functionName.Contains("ppcb");
            AppendIndentedLine((isPp ? "PP " : "CP ") + _functionName);
            if (isPp)
            {
                _summary.nrOfPpFunctions++;
            }
            else
            {
                _summary.nrOfCpFunctions++;
            }

            _functionName = "";
            IncreaseIndent();
        }


        private void PrintPl()
        {
            _indent = 0;
            AppendIndentedLine("PL");
            IncreaseIndent();
        }


        private void PrintCl()
        {
            _indent = 0;
            AppendIndentedLine("CL");
            IncreaseIndent();
        }


        private void PrintDecisionName(string line)
        {
            string[] separators = { "->", "_result" };
            AppendIndentedLine("DEC: " + Split(separators, line)[1]);
            IncreaseIndent();
            _summary.nrOfDecisions++;
        }


        private void PrintDecisionBranchName(string line)
        {
            DecreaseIndent();
            string[] separators = { "xDEC_", ")" };
            AppendIndentedLine("\\BRANCH: " + Split(separators, line)[1]);
            IncreaseIndent();
            _summary.nrOfBranches++;
        }


        private void PrintActionName(string line)
        {
            if (line.Contains("temp_physical_params_p->params.u.qssa_"))
            {
                PrintQssaName(line);
                _summary.nrOfQssa++;
            }
            else if (line.Contains("temp_physical_params_p->params.u.ssa_"))
            {
                PrintSsaName(line);
                _summary.nrOfSsa++;
            }
            else if (line.Contains("temp_physical_params_p->params.u.sa_"))
            {
                PrintSaName(line);
                _summary.nrOfSa++;
            }
            else if (line.Contains("temp_physical_params_p->params.u.dictatorship_"))
            {
                PrintDictatorShipName(line);
                _summary.nrOfSa++;
            }
        }

        private void PrintQssaName(string line)
        {
            string[] separators = { "temp_physical_params_p->params.u.qssa_",
                ".scan_timing.prep_time =" };
            AppendIndentedLine("QSSA: " + Split(separators, line)[1]);
        }


        private void PrintSsaName(string line)
        {
            string[] separators = { "temp_physical_params_p->params.u.ssa_",
                ".scan_timing.prep_time =" }; ;
            AppendIndentedLine("SSA: " + Split(separators, line)[1]);
        }


        private void PrintSaName(string line)
        {
            string[] separators = { "temp_physical_params_p->params.u.sa_",
                ".scan_timing.prep_time =" };
            AppendIndentedLine("SA: " + Split(separators, line)[1]);
        }


        private void PrintDictatorShipName(string line)
        {
            string[] separators = { "temp_physical_params_p->params.u.dictatorship_",
                ".scan_timing.prep_time =" };
            AppendIndentedLine("DICTATORSHIP: " + Split(separators, line)[1]);
        }


        private void PrintWaitForResultTrue()
        {
            IncreaseIndent();
            AppendIndentedLine("WaitForResult = TRUE");
            DecreaseIndent();
        }


        private void PrintScanType(string line)
        {
            IncreaseIndent();
            string[] separators = { ".scan_type = SNXA_", ";" };
            AppendIndentedLine("scan type: " + Split(separators, line)[1]); ;
            DecreaseIndent();
        }


        private void PrintPpName(string line)
        {
            string[] separators = { "(void *) ", ");" };
            var split = Split(separators, line);
            if (!split[1].Equals("NULL"))
            {
                IncreaseIndent();
                AppendIndentedLine("PP: " + split[1]);
                DecreaseIndent();
                _summary.nrOfPps++;
            }
        }

        private void PrintCpName(string line)
        {
            string[] separators = { "(void *) ", ");" };
            var split = Split(separators, line);
            if (!split[1].Equals("NULL"))
            {
                IncreaseIndent();
                AppendIndentedLine("CP: " + split[1]);
                DecreaseIndent();
                _summary.nrOfCps++;
            }
        }


        private void PrintDtfName(string line)
        {
            if (line.Contains("DTF_typeconversion_"))
            {
                string[] separators = { "DTF_typeconversion_", "(" };
                IncreaseIndent();
                AppendIndentedLine("DTF TYPE CONV: " + Split(separators, line)[1]);
                DecreaseIndent();
                _summary.nrOfTypeConversionDtfs++;

            }
            else
            {
                string[] separators = { "DTF_", "(" };
                AppendIndentedLine("DTF: " + Split(separators, line)[1]);
                _summary.nrOfDtfs++;

                if (line.Contains("(buf_action_timing_result,"))
                {
                    IncreaseIndent();
                    AppendIndentedLine("Action Timing Data Flow");
                    DecreaseIndent();
                }
            }
        }


        private void PrintLoopName(string line)
        {
            string[] separators = { "for (; " };
            AppendIndentedLine("LOOP: " + Split(separators, line)[1]);
            IncreaseIndent();
            _summary.nrOfLoops++;
        }


        private void PrintInterruptableLoop(string line)
        {
            if (_showLoopDetails)
            {
                string[] separators = { ">= (logical_action_p->nr_iterations_" };
                IncreaseIndent();
                AppendIndentedLine("loop interrupt check: " +
                    Split(separators, line)[1]);
            }
        }


        private void PrintSkipWholeLoop()
        {
            if (_showLoopDetails)
            {
                AppendIndentedLine("if SkipWholeLoop:");
                IncreaseIndent();
            }
        }


        private void PrintSetNextAction(string line)
        {
            if (_showLoopDetails)
            {
                var action = "(none)";
                if (!line.Contains("\"\""))
                {
                    string[] separators = { "\"", "." };
                    action = Split(separators, line)[1];
                }

                AppendIndentedLine("Set next action: " + action);
            }
        }


        private void PrintSkipNothing()
        {
            if (_showLoopDetails)
            {
                DecreaseIndent();
                AppendIndentedLine("if SkipNothing:");
                IncreaseIndent();
            }
        }


        private void PrintLoopInterruptConditionCheck(string line)
        {
            if (_showLoopDetails)
            {
                string[] separators = { "== logical_action_p->nr_iterations_ExpansionRegion", ")" };
                AppendIndentedLine("Set next action: " + Split(separators, line)[1]);
                DecreaseIndent();
            }
        }


        private void PrintSkipActions()
        {
            if (_showLoopDetails)
            {
                AppendIndentedLine("else:");
                IncreaseIndent();
                AppendIndentedLine("Skip actions");
            }
        }


        private void PrintLoopIterations0(string line)
        {
            if (_showLoopDetails)
            {
                string[] separators = { "&& (logical_action_p->nr_iterations_ExpansionRegion", "== 0))" };
                AppendIndentedLine("Check 0 iterations: " + Split(separators, line)[1]);
                IncreaseIndent();
            }
        }
  

        private void PrintExpansionRegionPpcb(string line)
        {
            string[] separators = { "result = ExpansionRegion_", "(entry_id, " };
            AppendIndentedLine("ExpansionRegion PPCB call: " + Split(separators, line)[1]);
        }


        private string[] Split(string[] separators, string line)
        {
            var result = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return result;
        }


        private void IncreaseIndent()
        {
            _indent += IndentSize;
        }


        private void DecreaseIndent()
        {
            _indent -= IndentSize;
        }


        private string Indent()
        {
            return String.Format("{0,4}: ", _lineNumber) + new string(' ', _indent);
        }


        private void AppendIndentedLine(string str)
        {
            _output.Append(Indent());
            _output.AppendLine(str);
        }
    }
}
