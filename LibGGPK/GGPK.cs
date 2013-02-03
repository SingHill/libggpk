﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibGGPK
{
	/// <summary>
	/// Handles parsing the GGPK pack file.
	/// </summary>
	public class GGPK
	{
		/// <summary>
		/// Map of every records offsets in the pack file
		/// </summary>
		public Dictionary<long, BaseRecord> RecordOffsets;
		/// <summary>
		/// Root of the directory tree
		/// </summary>
		public DirectoryTreeNode DirectoryRoot;
		/// <summary>
		/// Root of the Free record list
		/// </summary>
		public LinkedList<FreeRecord> FreeRoot;
		/// <summary>
		/// An estimation of the number of records in the Contents.GGPK file. This is only
		/// used to inform the users of the parsing progress.
		/// </summary>
		private const int EstimatedFileCount = 175000;

		public GGPK()
		{
			RecordOffsets = new Dictionary<long, BaseRecord>(EstimatedFileCount);
		}

		/// <summary>
		/// Iterates through the pack file and records the offsets and headers of each record found
		/// </summary>
		/// <param name="pathToGgpk">Path to pack file to read</param>
		/// <param name="output">Output function</param>
		private void ReadRecordOfsets(string pathToGgpk, Action<string> output)
		{
			float previousPercentComplete = 0.0f;

			using (FileStream fs = File.Open(pathToGgpk, FileMode.Open))
			{
				BinaryReader br = new BinaryReader(fs);

				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					long currentOffset = br.BaseStream.Position;
					BaseRecord record = RecordFactory.ReadRecord(br);
					RecordOffsets.Add(currentOffset, record);

					float percentComplete = br.BaseStream.Position / (float)br.BaseStream.Length;
					if (percentComplete - previousPercentComplete >= 0.10f)
					{
						output(String.Format("{0:00.00}%\n", 100.0 * percentComplete));
						previousPercentComplete = percentComplete;
					}
				}
				output(String.Format("{0:00.00}%\n", 100.0f*br.BaseStream.Position / (float)br.BaseStream.Length));
			}


		}

		/// <summary>
		/// Creates a directory tree using the parsed record headers and offsets
		/// </summary>
		/// <param name="output">Output function</param>
		private void CreateDirectoryTree(Action<string> output)
		{
			DirectoryRoot = DirectoryTreeMaker.BuildDirectoryTree(RecordOffsets);
			FreeRoot = FreeListMaker.BuildFreeList(RecordOffsets);
		}

		/// <summary>
		/// Parses the GGPK pack file and builds a directory tree from it.
		/// </summary>
		/// <param name="pathToGgpk">Path to pack file to read</param>
		/// <param name="output">Output function</param>
		public void Read(string pathToGgpk, Action<string> output)
		{
			output("Parsing GGPK...\n");
			ReadRecordOfsets(pathToGgpk, output);
			output("\n");
			output("Building directory tree...\n");
			CreateDirectoryTree(output);
		}
	}
}
