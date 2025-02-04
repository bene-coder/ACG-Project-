using Abp.Application.Services;

namespace ACGProjectGoGlobal
{
    /// <summary>
    /// Derive your application services from this class.
    /// How do i go about it
    /// </summary>
    public abstract class ACGProjectGoGlobalAppServiceBase : ApplicationService
    {
        protected ACGProjectGoGlobalAppServiceBase()
        {
            LocalizationSourceName = ACGProjectGoGlobalConsts.LocalizationSourceName;
        }
    }
}