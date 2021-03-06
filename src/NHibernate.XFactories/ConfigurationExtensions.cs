﻿// ================================================================= //
// Copyright (c) roydukkey. All rights reserved.                     //
// Licensed under the MIT license.                                   //
// See the LICENSE file in the project root for more information.    //
// ================================================================= //

namespace NHibernate.XFactories
{
	using Cfg;
	using Cfg.ConfigurationSchema;
	using System;
	using System.IO;
	using System.Xml;

	public static class ConfigurationExtensions
	{
		public const string ConfigMutationSuffix = "-x-factories";
		private const string ChildPrefixPath = CfgXmlHelper.CfgNamespacePrefix + ":";
		private const string RootPrefixPath = "//" + ChildPrefixPath;

		/// <summary>
		///		Configure NHibernate from a specified session-factory.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="fileName">System location of '.cfg.xml' configuration file.</param>
		/// <param name="factoryName">Name value of the desired session-factory.</param>
		/// <returns></returns>
		public static Configuration Configure(this Configuration config, string fileName, string factoryName)
		{
			// Load Configuration XML
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);

			return config.Configure(PrepareConfiguration(doc, factoryName));
		}

		/// <summary>
		///		Configure NHibernate from a specified session-factory.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="textReader">The XmlReader containing the hibernate-configuration.</param>
		/// <param name="factoryName">Name value of the desired session-factory.</param>
		/// <returns></returns>
		public static Configuration Configure(this Configuration config, XmlReader textReader, string factoryName)
		{
			// Load Configuration XML
			XmlDocument doc = new XmlDocument();
			doc.Load(textReader);

			return config.Configure(PrepareConfiguration(doc, factoryName));
		}

		/// <summary>
		///		Parses the configuration xml and ensures the target session-factory is the only one included.
		/// </summary>
		/// <param name="doc">The XmlDocument containing the hibernate-configuration.</param>
		/// <param name="factoryName">Name value of the desired session-factory.</param>
		/// <returns></returns>
		private static XmlTextReader PrepareConfiguration(XmlDocument doc, string factoryName)
		{
			// Add Proper Namespace
			XmlNamespaceManager namespaceMgr = new XmlNamespaceManager(doc.NameTable);
			namespaceMgr.AddNamespace(CfgXmlHelper.CfgNamespacePrefix, CfgXmlHelper.CfgSchemaXMLNS + ConfigMutationSuffix);

			// Query Elements
			XmlNode nhibernateNode = doc.SelectSingleNode(RootPrefixPath + CfgXmlHelper.CfgSectionName + ConfigMutationSuffix, namespaceMgr);

			if (nhibernateNode != null) {
				if (nhibernateNode.SelectSingleNode(RootPrefixPath + $"session-factory[@name='{factoryName}']", namespaceMgr) != default(XmlNode)) {
					foreach (XmlNode node in nhibernateNode.SelectNodes(RootPrefixPath + $"session-factory[@name!='{factoryName}']", namespaceMgr)) {
						nhibernateNode.RemoveChild(node);
					}
				}
				else {
					throw new Exception($"<session-factory name=\"{factoryName}\"> element was not found in the configuration file.");
				}
			}
			else {
				throw new Exception($"<{CfgXmlHelper.CfgSectionName + ConfigMutationSuffix} xmlns=\"{CfgXmlHelper.CfgSchemaXMLNS + ConfigMutationSuffix}\"> element was not found in the configuration file.");
			}

			return new XmlTextReader(new StringReader(nhibernateNode.OuterXml.Replace(ConfigMutationSuffix, "")));
		}
	}
}
