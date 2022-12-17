using System.ServiceModel;

namespace Lib.Common.Components.Agreements
{
    [ServiceContract(Namespace = "http://entry.serviceengine.cross.digiwin.com")]
    public interface ISOAP
    {
        [OperationContract(Name = "invokeSrv")]
        string InvokeSrv(string in0);
    }

    public interface IConstruction
    {
        public void Start();
    }
}
