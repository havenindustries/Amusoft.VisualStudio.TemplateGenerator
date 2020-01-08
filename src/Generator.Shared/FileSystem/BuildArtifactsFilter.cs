using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;

namespace Generator.Shared.FileSystem
{
	public class BuildArtifactsFilter : FileWalkerFilter
	{
		private readonly HashSet<Uri> Ignored = new HashSet<Uri>();

		/// <inheritdoc />
		public override void Initialize(string root)
		{
			Ignored.Clear();

			var projectFiles = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories);
			var projects = projectFiles.Select(GetEvaluationProject);
			foreach (var project in projects)
			{
				var bin = Path.Combine(project.DirectoryPath, project.GetPropertyValue("BaseOutputPath")
					.TrimEnd(Path.DirectorySeparatorChar))
					+ Path.DirectorySeparatorChar;
				var obj = Path.Combine(project.DirectoryPath, project.GetPropertyValue("BaseIntermediateOutputPath")
					.TrimEnd(Path.DirectorySeparatorChar))
					+ Path.DirectorySeparatorChar;

				Ignored.Add(new Uri(bin));
				Ignored.Add(new Uri(obj));
			}
		}

		private static Project GetEvaluationProject(string file)
		{
			return ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(d => d.FullPath == file)
				   ?? Project.FromFile(file, new ProjectOptions());
		}

		/// <inheritdoc />
		public override bool IsValid(string file)
		{
			var uri = new Uri(file, UriKind.Absolute);
		
			var pathToFile = uri.ToString();
			var subPath = @"/bin/";
			pathToFile = Path.GetDirectoryName(pathToFile) + "\\";
			string searchPath = Path.GetDirectoryName(subPath) + "\\";
			bool containsIt = pathToFile.IndexOf(searchPath, StringComparison.OrdinalIgnoreCase) > -1;

			if (containsIt)
			{
				return false;
			}

			subPath = @"/obj/";
			pathToFile = Path.GetDirectoryName(pathToFile) + "\\";
			searchPath = Path.GetDirectoryName(subPath) + "\\";
			containsIt = pathToFile.IndexOf(searchPath, StringComparison.OrdinalIgnoreCase) > -1;

			if (containsIt)
			{
				return false;
			}

			return true;
			
			//return Ignored.All(ignored => !ignored.IsBaseOf(uri));
		}
	}
}