using System;
using System.Collections.Generic;
using System.Linq;

namespace MIPSInterpreter
{
    public class DecompManager
    {
        public int? interpretedInstructionsOffset;
        public byte[] interpretedInstructions;
        public int? gControllerPads;

        private static readonly int OsContGetReadData1Offset = 5;
        private static readonly uint[] OsContGetReadData1 = new uint[14]
        {
            2947416076u, 434110497u, 2946498560u, 2411266060u, 666370052u, 2334195712u, 2602631171u, 2936078336u,
            2334720004u, 2603155463u, 2936602628u, 2477391878u, 826998976u, 745731u
        };

        private static readonly int OsContGetReadData2Offset = 6;
        private static readonly uint[] OsContGetReadData2 = new uint[29]
        {
            432013338u, 6181u, 665190404u, 2285961216u, 2554396675u, 2898329600u, 2287468548u, 2555904007u,
            2899836932u, 2478374918u, 858259648u, 542979u, 824836351u, 356515847u, 2693332996u, 2544566280u,
            2760572928u, 2209087498u, 2693529602u, 2209153035u, 2693595139u, 2427322368u, 610467841u, 608305160u,
            7211050u, 337706985u, 612630534u, 65011720u, 666697744u
        };

        private static readonly int OsContGetReadData3Offset = 6;
        private static readonly uint[] OsContGetReadData3 = new uint[12]
        {
            2422341634u, 2489843716u, 135427u, 809631756u, 2154233862u, 2154168327u, 614793217u, 339738628u,
            2692874246u, 2760376320u, 2693201922u, 2693136387u
        };

        private static uint[][] OsContGetReadData = new uint[3][]
        {
            OsContGetReadData1,
            OsContGetReadData2,
            OsContGetReadData3
        };

        private static int[] OsContGetReadDataOffsets = new int[3]
        {
            OsContGetReadData1Offset,
            OsContGetReadData2Offset,
            OsContGetReadData3Offset
        };

        private static unsafe List<int> IndicesOf(uint[] arrayToSearchThrough, uint[] patternToFind)
        {
            List<int> list = new List<int>();
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return list;

            fixed (uint* arrayPtr = arrayToSearchThrough)
            fixed (uint* patternPtr = patternToFind)
            {
                for (int i = 0; i <= arrayToSearchThrough.Length - patternToFind.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < patternToFind.Length; j++)
                    {
                        if (arrayPtr[i + j] != patternPtr[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                        list.Add(i);
                }
            }
            return list;
        }

        private static unsafe List<int> FindAll(uint[] arrayToSearchThrough, uint val)
        {
            List<int> list = new List<int>();
            fixed (uint* ptr = arrayToSearchThrough)
            {
                for (int i = 0; i < arrayToSearchThrough.Length; i++)
                {
                    if (ptr[i] == val)
                        list.Add(i);
                }
            }
            return list;
        }

        private static bool IsVAddr(uint addr)
        {
            return (addr & 0xFF000000) == 0x80000000 && (addr & 0x00FFFFFF) <= 0x00800000;
        }

        public DecompManager(uint[] mem)
        {
            List<int> candidates = new List<int>();

            for (int i = 0; i < OsContGetReadData.Length; i++)
            {
                foreach (int index in IndicesOf(mem, OsContGetReadData[i]))
                {
                    candidates.Add(index - OsContGetReadDataOffsets[i]);
                }
            }

            if (candidates.Count == 0)
                throw new ArgumentException("Failed to find osContGetReadData!");

            foreach (int candidate in candidates)
            {
                uint jalInstruction = Converter.ToUInt(new Instruction
                {
                    cmd = Cmd.JAL,
                    jump = (uint)(candidate * 4)
                });

                foreach (int jalIndex in FindAll(mem, jalInstruction))
                {
                    try
                    {
                        uint entryPoint = 0x80000000u | ((uint)jalIndex << 2);

                        Interpreter interpreter = new Interpreter(mem);
                        uint preloadCount = 16;
                        interpreter.pc = entryPoint - (preloadCount << 2);

                        for (int i = 0; i < preloadCount + 2; i++)
                        {
                            Instruction? inst = interpreter.GetInstruction();
                            if (inst.HasValue)
                                interpreter.Execute(inst.Value);
                        }

                        int padsAddr = interpreter.gpr[4]; // a0 holds the pads pointer

                        if (IsVAddr((uint)padsAddr))
                        {
                            uint segmentStart = (uint)jalIndex - preloadCount;
                            interpretedInstructionsOffset = (int)(segmentStart << 2);

                            ArraySegment<uint> segment = new ArraySegment<uint>(mem, (int)segmentStart, (int)preloadCount);
                            interpretedInstructions = new byte[preloadCount << 2];
                            int destIdx = 0;
                            foreach (uint word in segment)
                            {
                                Array.Copy(BitConverter.GetBytes(word), 0, interpretedInstructions, destIdx, 4);
                                destIdx += 4;
                            }

                            gControllerPads = padsAddr;
                            return;
                        }
                    }
                    catch
                    {
                        // Ignore bad interpretations
                    }
                }
            }
        }
    }
}