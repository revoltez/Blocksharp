using Akka.Actor;

namespace PBFT
{
    class PropsConfig
    {
       public static Props props<T>() where T: ActorBase{
           return Props.Create<T>();
       }
       
        public static Props props<T>(IActorRef reference) where T: ActorBase{
           return Props.Create<T>(reference);
       }

    }
}