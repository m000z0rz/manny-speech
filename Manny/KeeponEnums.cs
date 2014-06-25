namespace Manny.Keepon
{
    public enum KeeponMotorType
    {
        Pan,
        Tilt,
        PonSide
    }

    public enum KeeponMovementType
    {
        Pan,
        Tilt,
        Pon,
        Side
    }

    public enum KeeponSideTarget
    {
        None,
        Center,
        Left,
        Right
    }

    public enum KeeponSideMovement
    {
        CenterFromLeft,
        CenterFromRight,
        Left,
        Right
        //Cycle
    }

    public enum KeeponPonTarget
    {
        None,
        Up,
        HalfDown,
        Down,
        HalfUp
    }

    public enum KeeponTiltEncoder
    {
        NoReach,
        Forward,
        Back,
        Up
    }

    public enum KeeponSideEncoder
    {
        Center,
        Right,
        Left
    }

    public enum KeeponPanEncoder
    {
        Back,
        Right,
        Left,
        Center
    }

    public enum KeeponPonEncoder
    {
        HalfDown,
        Up,
        Down,
        HalfUp
    }

    public enum KeeponMotorState
    {
        Unknown,
        Finished,
        Stalled
    }

    public enum KeeponButton
    {
        Head,
        Front,
        Back,
        Left,
        Right,
        Dance,
        Touch
    }

}