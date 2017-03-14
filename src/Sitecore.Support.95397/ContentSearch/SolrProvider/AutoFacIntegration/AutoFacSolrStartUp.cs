namespace Sitecore.Support.ContentSearch.SolrProvider.AutoFacIntegration
{
  using Autofac;
  using Autofac.Builder;
  using AutofacContrib.CommonServiceLocator;
  using AutofacContrib.SolrNet;
  using AutofacContrib.SolrNet.Config;
  using Microsoft.Practices.ServiceLocation;
  using Sitecore.ContentSearch;
  using Sitecore.ContentSearch.SolrProvider;
  using Sitecore.ContentSearch.SolrProvider.DocumentSerializers;
  using SolrNet;
  using SolrNet.Impl;
  using SolrNet.Schema;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class AutoFacSolrStartUp : ISolrStartUp, IProviderStartUp
  {
    private readonly ContainerBuilder builder;
    private readonly SolrServers Cores;
    private IContainer container;

    public AutoFacSolrStartUp(ContainerBuilder builder)
    {
      if (!SolrContentSearchManager.IsEnabled)
      {
        return;
      }

      this.builder = builder;
      this.Cores = new SolrServers();
    }

    private ISolrCoreAdmin BuildCoreAdmin()
    {
      SolrConnection solrConnection = new SolrConnection(SolrContentSearchManager.ServiceAddress);
      if (SolrContentSearchManager.EnableHttpCache)
      {
        solrConnection.Cache = this.container.Resolve<ISolrCache>() ?? new NullCache();
      }

      return new SolrCoreAdmin(solrConnection, this.container.Resolve<ISolrHeaderResponseParser>(), this.container.Resolve<ISolrStatusResponseParser>());
    }

    public void Initialize()
    {
      if (!SolrContentSearchManager.IsEnabled)
      {
        throw new InvalidOperationException("Solr configuration is not enabled. Please check your settings and include files.");
      }

      foreach (string str in SolrContentSearchManager.Cores)
      {
        this.AddCore(str, typeof(Dictionary<string, object>), SolrContentSearchManager.ServiceAddress + "/" + str);
      }

      this.builder.RegisterModule(new SolrNetModule(this.Cores));
      this.builder.RegisterType<SolrFieldBoostingDictionarySerializer>().As<ISolrDocumentSerializer<Dictionary<string, object>>>();
      this.builder.RegisterType<SolrSchemaParser>().As<ISolrSchemaParser>();
      if (SolrContentSearchManager.EnableHttpCache)
      {
        this.builder.RegisterType<HttpRuntimeCache>().As<ISolrCache>();
        foreach (SolrServerElement element in this.Cores)
        {
          string serviceName = element.Id + typeof(SolrConnection);
          NamedParameter[] parameters = new NamedParameter[] { new NamedParameter("serverURL", element.Url) };
          this.builder.RegisterType(typeof(SolrConnection)).Named(serviceName, typeof(ISolrConnection)).WithParameters(parameters)
            .OnActivated(args => ((SolrConnection)args.Instance).Cache = args.Context.Resolve<ISolrCache>());
        }
      }

      this.container = this.builder.Build(ContainerBuildOptions.None);
      ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(this.container));
      SolrContentSearchManager.SolrAdmin = this.BuildCoreAdmin();
      SolrContentSearchManager.Initialize();
    }

    public void AddCore(string coreId, Type documentType, string coreUrl)
    {
      SolrServers cores = this.Cores;
      SolrServerElement configurationElement = new SolrServerElement();
      configurationElement.Id = coreId;
      string assemblyQualifiedName = documentType.AssemblyQualifiedName;
      configurationElement.DocumentType = assemblyQualifiedName;
      string str = coreUrl;
      configurationElement.Url = str;
      cores.Add(configurationElement);
    }

    public bool IsSetupValid()
    {
      if (!SolrContentSearchManager.IsEnabled)
      {
        return false;
      }

      ISolrCoreAdmin admin = this.BuildCoreAdmin();
      return SolrContentSearchManager.Cores
        .Select(defaultIndex => admin.Status(defaultIndex).First())
        .All(status => status.Name != null);
    }
  }
}
