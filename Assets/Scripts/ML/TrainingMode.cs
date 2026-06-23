namespace BusBoys.Assets.Scripts.ML
{
    //Holds value for current training mode.
    public enum TrainingMode
    {
        SingleNode      = 0,   // reach 1 graph node, episode ends
        MultiNode       = 1,   // reach N graph nodes in sequence, episode ends
        SingleWaypoint  = 2,   // reach 1 bus-stop waypoint (short route), episode ends
        FullRoute       = 3,   // cycle through all waypoints indefinitely
    }
}
