﻿using Autofac;
using Sitecore.Web;

namespace Sitecore.Support.ContentSearch.SolrProvider.AutoFacIntegration
{
  public class AutoFacApplication : Application
  {
    public virtual void Application_Start()
    {
      new AutoFacSolrStartUp(new ContainerBuilder()).Initialize();
    }
  }
}