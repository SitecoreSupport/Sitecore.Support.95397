namespace Sitecore.Support.ContentSearch.SolrProvider.AutoFacIntegration
{
  using Autofac;
  using Sitecore.Web;

  public class AutoFacApplication : Application
  {
    public virtual void Application_Start()
    {
      new AutoFacSolrStartUp(new ContainerBuilder())
        .Initialize();
    }
  }
}