//------------------------------------------------------------------------
//   File: Sudoku.cs
//   Created On: 10/12/2007
//   Author(s): Tony Wang
//------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LogicalSudokuSolver
{
    class Sudoku
    {
        #region Members
        class Point
        {
            public int X, Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public int[][] data;
        public List<int>[][] possible = new List<int>[9][],
            hrules = new List<int>[9][],
            vrules = new List<int>[9][],
            srules = new List<int>[9][];
        Point[][] hrpoints = new Point[9][],
            vrpoints = new Point[9][],
            srpoints = new Point[9][];

        public bool debugmode = false, writeLog = false;
        StringBuilder content = new StringBuilder();
        #endregion

        #region Constructor
        /// <summary>
        /// initialize data/rules
        /// </summary>        
        public Sudoku()
        {
            //clear log.txt
            for (int i = 0; i < 9; i++)
            {
                possible[i] = new List<int>[9];

                hrpoints[i] = new Point[9];
                vrpoints[i] = new Point[9];
                srpoints[i] = new Point[9];

                for (int j = 0; j < 9; j++)
                {
                    possible[i][j] = new List<int>();
                    for (int k = 1; k < 10; k++)
                    {
                        possible[i][j].Add(k);
                    }
                }
            }

            for (int i = 0; i < 9; i++)
            {
                hrules[i] = new List<int>[9];
                vrules[i] = new List<int>[9];
                srules[i] = new List<int>[9];

                for (int j = 0; j < 9; j++)
                {
                    hrules[i][j] = possible[i][j];
                    hrpoints[i][j] = new Point(i, j);
                    vrules[i][j] = possible[j][i];
                    vrpoints[i][j] = new Point(j, i);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int k = i * 3 + j;
                    for (int ii = 0; ii < 3; ii++)
                    {
                        for (int jj = 0; jj < 3; jj++)
                        {
                            srules[k][ii * 3+ jj] = possible[i * 3 + ii][j * 3 + jj];
                            srpoints[k][ii * 3 + jj] = new Point(i * 3 + ii, j * 3 + jj);
                        }
                    }
                }
            }
        }
        #endregion

        #region Load
        /// <summary>
        /// clear what ever has data
        /// </summary>
        void Normalize()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (data[i][j] < 0 || data[i][j] > 9)
                    {
                        data[i][j] = 0;
                    }
                    else if (data[i][j] != 0)
                    {
                        possible[i][j].Clear();
                    }
                }
            }
        }

        /// <summary>
        /// assume input is [9][9]
        /// </summary>
        /// <param name="input"></param>
        public void Load(int[][] input)
        {
            data = input;

            //normalize data
            Normalize();
        }
        #endregion

        #region ValidateRule
        bool Validate()
        {
            for (int i = 0; i < 9; i++)
            {
                if (!ValidateRule(hrpoints[i])) return false;
                if (!ValidateRule(vrpoints[i])) return false;
                if (!ValidateRule(srpoints[i])) return false;
            }

            return true;
        }

        bool ValidateRule(Point[] points)
        {
            List<Point> plist = new List<Point>();
            foreach (Point p in points)
            {
                plist.Add(p);
            }
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 0; i < 9; i++)
            {
                int value = data[plist[i].X][plist[i].Y];
                if (value == 0) continue;

                if (dict.ContainsKey(value)) return false;
                else dict[value] = 0;
            }
            return true;
        }
        #endregion

        #region Finished
        /// <summary>
        /// check if data is filled
        /// </summary>
        public bool Finished()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (data[i][j] == 0) return false;
                }
            }

            return true;
        }
        #endregion

        #region Log
        public void LogLine(string format, params Object[] arg)
        {
            Console.WriteLine(format, arg);
            if (writeLog)
            {
                //StreamWriter writer = new StreamWriter("log.txt", true);
                //writer.WriteLine(format, arg);
                //writer.Close();
                content.Append(string.Format(format, arg));
                content.Append(Environment.NewLine);
            }
        }

        public void Log(string format, params Object[] arg)
        {
            Console.Write(format, arg);
            if (writeLog)
            {
                //StreamWriter writer = new StreamWriter("log.txt", true);
                //writer.Write(format, arg);
                //writer.Close();
                content.Append(string.Format(format, arg));
            }
        }
        #endregion

        #region Test
        public void Test()
        {
            int[][] input = new int[9][];

			input = ReadLine("7....29...3..4....2..1....8..5..7...42.....35...3..8..5....4..9....8..5...12....7");

            Load(input);
            Print();

            while (!Finished())
            {
                //Console.ReadKey();
                LogLine("\r\n=======================================\r\n");

                Eliminate();
                if (!Populate())
                {
                    PrintPossible();
                    LogLine("=======================================");
                    PrintPossible(false);

                    LogLine("\r\nSTUCK!!!\r\n");
                    Console.ReadKey();
                    break;
                }
                else
                {
                    if (debugmode)
                    {
                        PrintPossible();
                        LogLine("=======================================");
                    }
                    Print();

                    //if (debugmode) Console.ReadKey();
                }
            }

            if (Finished()) LogLine("\r\nFinished!!!");
        }

        ~Sudoku()
        {
            if (writeLog)
            {
                StreamWriter writer = new StreamWriter("log.txt");
                writer.Write(content.ToString());
                writer.Close();
            }
        }
        #endregion

        #region Print
        /// <summary>
        /// print data
        /// </summary>
        public void Print()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (data[i][j] == 0) Log(" ");
                    else Log(data[i][j].ToString());

                    if (j == 2 || j == 5) Log("|");
                }
                LogLine("");
                if (i == 2 || i == 5) LogLine("-----------");
            }
        }
        #endregion

        #region PrintPossible
        /// <summary>
        /// print rules
        /// </summary>
        public void PrintPossible()
        {
            PrintPossible(true);
        }

        public void PrintPossible(bool showdata)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int jj = 0; jj < 3; jj++)
                {
                    //for each line
                    for (int j = 0; j < 9; j++)
                    {
                        for (int ii = 0; ii < 3; ii++)
                        {
                            int k = ii + jj * 3 + 1;
                            if (possible[i][j].Contains(k))
                            {
                                Log(k.ToString());
                            }
                            else if (showdata && k == 5 && possible[i][j].Count == 0)
                            {
                                Log(data[i][j].ToString());
                            }
                            else
                            {
                                Log(" ");
                            }
                        }
                        Log(" ");
                        if (j == 2 || j == 5) Log("| ");
                    }
                    LogLine("");
                }

                if (i == 2 || i == 5) LogLine("---------------------------------------");
                else LogLine("");
            }
        }
        #endregion

        #region Eliminate
        /// <summary>
        /// remove anything that's already in data from rules
        /// </summary>
        public void Eliminate()
        {
            Normalize();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (hrules[i][j].Count == 0)
                    {
                        int value = data[hrpoints[i][j].X][hrpoints[i][j].Y];
                        for (int k = 0; k < 9; k++)
                        {
                            if (j != k)
                            {
                                hrules[i][k].Remove(value);
                            }
                        }
                    }
                    if (vrules[i][j].Count == 0)
                    {
                        int value = data[vrpoints[i][j].X][vrpoints[i][j].Y];
                        for (int k = 0; k < 9; k++)
                        {
                            if (j != k)
                            {
                                vrules[i][k].Remove(value);
                            }
                        }
                    }
                    if (srules[i][j].Count == 0)
                    {
                        int value = data[srpoints[i][j].X][srpoints[i][j].Y];
                        for (int k = 0; k < 9; k++)
                        {
                            if (j != k)
                            {
                                srules[i][k].Remove(value);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Help - CompareList
        bool CompareList(List<Point> list1, List<Point> list2)
        {
            if (list1.Count != list2.Count) return false;

            foreach (Point point in list1)
            {
                if (!list2.Contains(point))
                {
                    return false;
                }
            }
            return true;
        }

        bool CompareList(List<int> list1, List<int> list2)
        {
            if (list1.Count != list2.Count) return false;

            foreach (int value in list1)
            {
                if (!list2.Contains(value))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region PopulateRule
        bool PopulateRule(List<int>[][] rules, Point[][] points)
        {
            Dictionary<int, Point> checklist;
            bool ret = false;

            for (int i = 0; i < 9; i++)
            {
                checklist = new Dictionary<int, Point>();

                for (int j = 0; j < 9; j++)
                {

                    #region 1. fill only value
                    Point p = points[i][j];
                    if (rules[i][j].Count == 1 && data[p.X][p.Y] == 0)
                    {
                        data[p.X][p.Y] = rules[i][j][0];
                        ret = true;

                        if (!Validate())
                        {
                            LogLine("Only Value ERROR: {0},{1} - {2}",
                                points[i][j].X, points[i][j].Y, rules[i][j][0]);
                            LogLine("\r\n===================ERROR!!!===================\r\n");
                            PrintPossible(false);
                            return false;
                        }
                        else if (debugmode)
                        {
                            LogLine("Only Value: {0},{1} - {2}",
                                points[i][j].X, points[i][j].Y, rules[i][j][0]);
                        }
                    }
                    #endregion

                    #region check position
                    foreach (int value in rules[i][j])
                    {
                        if (!checklist.ContainsKey(value))
                        {
                            checklist[value] = points[i][j];
                        }
                        else if (checklist[value] != null)
                        {
                            if (checklist[value] != points[i][j])
                            {
                                checklist[value] = null;
                            }
                        }
                    }
                    #endregion
                }

                #region 2. fill only position
                foreach (int key in checklist.Keys)
                {
                    Point p = checklist[key];
                    if (p != null && data[p.X][p.Y] == 0)
                    {
                        data[p.X][p.Y] = key;
                        ret = true;

                        if (!Validate())
                        {
                            LogLine("Only Position ERROR: {0},{1} - {2}",
                                p.X, p.Y, key);
                            LogLine("\r\n===================ERROR!!!===================\r\n");
                            PrintPossible(false);
                            return false;
                        }
                        else if (debugmode)
                        {
                            LogLine("Only Position: {0},{1} - {2}",
                                p.X, p.Y, key);
                        }
                    }
                }
                #endregion

                #region 3. cyclic positions rules
                int count = 0;
                Dictionary<int, int> pcounts = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    if (rules[i][j].Count > 0) count++;

                    foreach (int value in rules[i][j])
                    {
                        if (!pcounts.ContainsKey(value)) pcounts[value] = 1;
                        else pcounts[value]++;
                    }
                }

                for (int k = 2; k < count; k++)
                {
                    List<int> pmatches = new List<int>();

                    //cyclic positions
                    //check those positions with 2,3,x,x 2,3,x,x pair etc
                    foreach (int key in pcounts.Keys)
                    {
                        if (pcounts[key] == k)
                        {
                            pmatches.Add(key);
                        }
                    }

                    Dictionary<int, List<Point>> pdict = new Dictionary<int, List<Point>>();
                    if (pmatches.Count >= k)
                    {
                        foreach (int key in pmatches)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                if (rules[i][j].Contains(key))
                                {
                                    if (pdict.ContainsKey(key))
                                    {
                                        pdict[key].Add(points[i][j]);
                                    }
                                    else
                                    {
                                        List<Point> list = new List<Point>();
                                        list.Add(points[i][j]);
                                        pdict[key] = list;
                                    }
                                }
                            }
                        }

                        //check the lists in pdict
                        List<List<int>> results = new List<List<int>>();
                        foreach (int key1 in pdict.Keys)
                        {
                            foreach (int key2 in pdict.Keys)
                            {
                                if (key1 != key2 && CompareList(pdict[key1], pdict[key2]))
                                {
                                    bool check = true;
                                    foreach (List<int> result in results)
                                    {
                                        if (result.Contains(key1) || result.Contains(key2))
                                        {
                                            check = false;
                                            if (!result.Contains(key1)) result.Add(key1);
                                            if (!result.Contains(key2)) result.Add(key2);
                                            break;
                                        }
                                    }
                                    if (check)
                                    {
                                        List<int> result = new List<int>();
                                        result.Add(key1);
                                        result.Add(key2);
                                        results.Add(result);
                                    }
                                }
                            }
                        }

                        foreach (List<int> result in results)
                        {
                            if (result.Count == k)
                            {
                                //position cyclic rule found
                                if (debugmode)
                                {
                                    Log("Found Cyclic Positions: ");
                                    foreach (int value in result)
                                    {
                                        Log(value.ToString());
                                        Log(" ");
                                    }
                                    Log(" At ");
                                }
                                for (int j = 0; j < 9; j++)
                                {
                                    if (rules[i][j].Contains(result[0]) && rules[i][j].Count > k)
                                    {
                                        //remove other values in those positions
                                        rules[i][j].Clear();
                                        foreach (int value in result)
                                        {
                                            rules[i][j].Add(value);
                                        }
                                        if (debugmode)
                                        {
                                            Log("{0},{1} ", points[i][j].X, points[i][j].Y);
                                        }
                                    }
                                }
                                if (debugmode) LogLine("");
                            }
                        }
                    }
                }
                #endregion

                #region 4. cyclic values rules
                for (int k = 2; k < count; k++)
                {
                    List<List<int>> vmatches = new List<List<int>>();

                    for (int j = 0; j < 9; j++)
                    {
                        //check those values such as 2,3 2,3 pair etc
                        if (rules[i][j].Count == k)
                        {
                            vmatches.Add(rules[i][j]);
                        }
                    }

                    if (vmatches.Count >= k)
                    {
                        List<List<List<int>>> results = new List<List<List<int>>>();
                        foreach (List<int> list1 in vmatches)
                        {
                            foreach (List<int> list2 in vmatches)
                            {
                                if (list1 != list2 && CompareList(list1, list2))
                                {
                                    bool check = true;
                                    foreach (List<List<int>> result in results)
                                    {
                                        if (result.Contains(list1) || result.Contains(list2))
                                        {
                                            check = false;
                                            if (!result.Contains(list1)) result.Add(list1);
                                            if (!result.Contains(list2)) result.Add(list2);
                                            break;
                                        }
                                    }
                                    if (check)
                                    {
                                        List<List<int>> result = new List<List<int>>();
                                        result.Add(list1);
                                        result.Add(list2);
                                        results.Add(result);
                                    }
                                }
                            }
                        }

                        foreach (List<List<int>> result in results)
                        {
                            if (result.Count == k)
                            {
                                List<int> list1 = result[0];
                                if (debugmode)
                                {
                                    Log("Found Cyclic Values: ");
                                    foreach (int value in list1)
                                    {
                                        Log(value.ToString());
                                        Log(" ");
                                    }
                                    Log(" At ");
                                }
                                for (int j = 0; j < 9; j++)
                                {
                                    if (!CompareList(rules[i][j], list1))
                                    {
                                        foreach (int value in list1)
                                        {
                                            rules[i][j].Remove(value);
                                        }
                                    }
                                    else if (debugmode) 
                                    {
                                        Log("{0},{1} ", points[i][j].X, points[i][j].Y);
                                    }
                                }
                                if (debugmode) LogLine("");
                            }
                        }
                    }
                }
                #endregion
            }

            return ret;
        }
        #endregion

        #region Populate
        /// <summary>
        /// populate single value in rules
        /// </summary>
        public bool Populate()
        {
            bool ret = false;

            if (debugmode)
            {
                LogLine("Check horizontal rules");
                Print();
                LogLine("=========================================");
                PrintPossible(false);
            }
            if (PopulateRule(hrules, hrpoints)) ret = true;
            if (debugmode)
            {
                LogLine("Check vertical rules");
                Print();
                LogLine("=========================================");
                PrintPossible(false);
            }
            if (PopulateRule(vrules, vrpoints)) ret = true;
            if (debugmode)
            {
                LogLine("Check square rules");
                Print();
                LogLine("=========================================");
                PrintPossible(false);
            }
            if (PopulateRule(srules, srpoints)) ret = true;

            if (debugmode)
            {
                LogLine("Check Square rule interact with other rules");
                Print();
                LogLine("=========================================");
                PrintPossible(false);
            }

            #region 5. Square rule interact with other rules
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> sdict = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int value in srules[i][j])
                    {
                        if (!sdict.ContainsKey(value)) sdict[value] = 1;
                        else sdict[value]++;
                    }
                }

                foreach (int value in sdict.Keys)
                {
                    if (sdict[value] == 2 || sdict[value] == 3)
                    {
                        int[] counts = new int[6];
                        #region check line count
                        if (srules[i][0].Contains(value))
                        {
                            counts[0]++;
                            counts[3]++;
                        }
                        if (srules[i][1].Contains(value))
                        {
                            counts[0]++;
                            counts[4]++;
                        }
                        if (srules[i][2].Contains(value))
                        {
                            counts[0]++;
                            counts[5]++;
                        }
                        if (srules[i][3].Contains(value))
                        {
                            counts[1]++;
                            counts[3]++;
                        }
                        if (srules[i][4].Contains(value))
                        {
                            counts[1]++;
                            counts[4]++;
                        }
                        if (srules[i][5].Contains(value))
                        {
                            counts[1]++;
                            counts[5]++;
                        }
                        if (srules[i][6].Contains(value))
                        {
                            counts[2]++;
                            counts[3]++;
                        }
                        if (srules[i][7].Contains(value))
                        {
                            counts[2]++;
                            counts[4]++;
                        }
                        if (srules[i][8].Contains(value))
                        {
                            counts[2]++;
                            counts[5]++;
                        }
                        #endregion

                        for (int ii = 0; ii < 3; ii++)
                        {
                            int x = i / 3, y = i % 3;
                            if (counts[ii] == sdict[value])
                            {
                                List<List<int>> lists = new List<List<int>>();
                                lists.Add(srules[i][ii * 3]);
                                lists.Add(srules[i][ii * 3 + 1]);
                                lists.Add(srules[i][ii * 3 + 2]);

                                if (debugmode) LogLine("Square rule {0} interacts with horizontal rule {1} for {2}",
                                    i, x * 3 + ii, value);

                                //remove other values in hrules
                                foreach (List<int> rule in hrules[x * 3 + ii])
                                {
                                    if (!lists.Contains(rule)) rule.Remove(value);
                                }
                            }
                            else if (counts[ii + 3] == sdict[value])
                            {
                                List<List<int>> lists = new List<List<int>>();
                                lists.Add(srules[i][ii]);
                                lists.Add(srules[i][ii + 3]);
                                lists.Add(srules[i][ii + 6]);
                                
                                if (debugmode) LogLine("Square rule {0} interacts with vertical rule {1}  for {2}",
                                    i, y * 3 + ii, value);

                                //remove other values in vrules
                                foreach (List<int> rule in vrules[y * 3 + ii])
                                {
                                    if (!lists.Contains(rule)) rule.Remove(value);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //LogLine("=========================================");
            //PrintPossible(false);

            return ret;
        }
        #endregion

		#region Read top1465 format data
		const int zero = (int)'0';
		int[][] ReadLine(string line)
        {
            int[][] data = new int[9][];

            for (int i = 0; i < 9; i++)
            {
                data[i] = new int[9];
                for (int j = 0; j < 9; j++)
                {
                    char c = line[i * 9 + j];
                    if (c == '.') data[i][j] = 0;
                    else data[i][j] = (int)c - zero;
                }
            }
            return data;
        }
        #endregion
    }
}
