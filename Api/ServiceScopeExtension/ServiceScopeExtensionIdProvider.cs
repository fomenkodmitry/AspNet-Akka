using Akka.Actor;

namespace Api.ServiceScopeExtension
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceScopeExtensionIdProvider : ExtensionIdProvider<Api.ServiceScopeExtension.ServiceScopeExtension>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public override Api.ServiceScopeExtension.ServiceScopeExtension CreateExtension(ExtendedActorSystem system)
        {
            return new Api.ServiceScopeExtension.ServiceScopeExtension();
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly ServiceScopeExtensionIdProvider Instance = new ServiceScopeExtensionIdProvider();
    }
}