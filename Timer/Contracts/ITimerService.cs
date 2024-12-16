using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface ITimerService
    {
        [OperationContract]
        void TestCommunication();
        
        /// <summary>
        /// Pokrece tajmer. Zahteva StartStop permisiju.
        /// </summary>
        [OperationContract]
        void StartTimer();

        /// <summary>
        /// Zaustavlja tajmer. Zahteva StartStop permisiju.
        /// </summary>
        [OperationContract]
        void StopTimer();

        /// <summary>
        /// Ponistava tajmer (postavlja na nulu). Zahteva Change permisiju.
        /// </summary>
        [OperationContract]
        void ResetTimer();

        /// <summary>
        /// Postavlja vrednost tajmera. Vrednost treba poslati sifrovano koristeci 3DES algoritam u ECB modu.
        /// Zahteva Change permisiju.
        /// </summary>
        /// <param name="encryptedTime">sifrovano vreme.</param>
        [OperationContract]
        void SetTimer(string encryptedTime);

        /// <summary>
        /// Ocitava trenutno vreme na tajmeru. Zahteva Seek permisiju.
        /// </summary>
        /// <returns>Trenutno vreme tajmera.</returns>
        [OperationContract]
        string AskForTime();
    }
}
