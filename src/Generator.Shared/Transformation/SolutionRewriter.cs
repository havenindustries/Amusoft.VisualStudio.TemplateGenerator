﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Generator.Shared.Resources;
using Generator.Shared.Serialization;
using Generator.Shared.Template;
using Generator.Shared.Utilities;
using NLog;

namespace Generator.Shared.Transformation
{
	public class SolutionRewriter : RewriterBase
	{
		private static readonly ILogger Log = LogManager.GetLogger(nameof(SolutionRewriter));

		public SolutionRewriteContext Context { get; }

		public string Folder { get; }

		public SolutionRewriter(SolutionRewriteContext context, string folder)
		{
			Context = context;
			Folder = folder;
		}

		public async Task<bool> RewriteAsync()
		{
			Log.Debug($"Rewriting folder \"{Folder}\".");
			if (!MoveContentFiles(out var contentFolder))
				return false;

			var solutionFile = FileHelper.FindSolutionAsync(contentFolder);
			if (!File.Exists(solutionFile))
			{
				Log.Error($"Solution file at \"{contentFolder}\" not found.");
				return false;
			}

			var explorer = await SolutionExplorer.CreateAsync(solutionFile, Context.Progress, Context.CancellationToken);
			var rootTemplatePath = Path.Combine(Folder, "root.vstemplate");

			var projectRewriteCache = new ProjectRewriteCache(explorer, rootTemplatePath);
			projectRewriteCache.Build();
			try
			{
				CreateRootVsTemplate(Context, projectRewriteCache, rootTemplatePath);
			}
			catch (Exception e)
			{
				Context.Progress.Report($"Failed to create {rootTemplatePath}.");
				await Task.Delay(3000);
				Log.Error(e);
				return false;
			}

			foreach (var pair in explorer.ProjectsLookup)
			{
				var context = new ProjectRewriteContext(projectRewriteCache, pair.Key, Context.CancellationToken, Context.Configuration, explorer);
				ProjectRewriter rewriter = new ProjectRewriter(context);
				await rewriter.ExecuteAsync();
			}

			Log.Info($"Rewriting complete.");
			return true;
		}

		private void CreateRootVsTemplate(SolutionRewriteContext context, ProjectRewriteCache cache, string templatePath)
		{
			var template = new VsTemplate();
			template.Type = Constants.VsTemplate.ProjectTypes.ProjectGroup;
			template.TemplateData.CreateInPlace = context.Configuration.CreateInPlace;
			template.TemplateData.CreateNewFolder = context.Configuration.CreateNewFolder;
			template.TemplateData.DefaultName = context.Configuration.DefaultName;
			template.TemplateData.Description = context.Configuration.Description;
			template.TemplateData.Name = context.Configuration.Name;
			template.TemplateData.ProvideDefaultName = context.Configuration.ProvideDefaultName;
			template.TemplateData.CodeLanguage = context.Configuration.CodeLanguage;
			template.TemplateData.Icon.Id = context.Configuration.IconPackageReference.Id;
			template.TemplateData.Icon.Package = context.Configuration.IconPackageReference.Package;
			
			template.TemplateContent = BuildRootTemplateContent(cache);

			SaveTemplate(template, templatePath);
		}

		private TemplateContent BuildRootTemplateContent(ProjectRewriteCache cache)
		{
			/**
			 *	root.vstemplate links like
			 *	<ProjectTemplateLink ProjectName="$safeprojectname$.Interface" CopyParameters="true">
				  Interface\InterfaceTemplate.vstemplate
				</ProjectTemplateLink>
			 */

			var content = new TemplateContent();
			var links = new List<NestableContent>();
			foreach (var reference in cache.GetSolutionProjectReferences())
			{
				links.Add(new ProjectTemplateLink(reference.RootTemplateNamespace, reference.RelativeVsTemplatePath));
			}

			content.Children = new NestableContent[]
			{
				new ProjectCollection(links.ToArray())
			};
			return content;
		}

		private bool MoveContentFiles(out string contentFolder)
		{
			// bypass folders which are named "Content too
			var tmpContent = Path.Combine(Folder, "___contents");
			if (!FileHelper.MoveContents(Folder, tmpContent))
			{
				Log.Error($"Failed to copy contents from \"{Folder}\" to \"{tmpContent}\".");
				contentFolder = null;
				return false;
			}

			contentFolder = Path.Combine(Folder, "Content");
			Directory.Move(tmpContent, contentFolder);
			return true;
		}
	}
}