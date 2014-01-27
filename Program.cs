using System;
using System.Collections.Generic;
using System.Text;

namespace LogicalSudokuSolver
{
	class Program
	{
		static void Main(string[] args)
		{
			Sudoku sudoku = new Sudoku();
			sudoku.debugmode = true;
			sudoku.writeLog = false;
			sudoku.Test();
		}
	}
}
