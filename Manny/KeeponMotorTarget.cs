using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Manny.Keepon
{
    class KeeponMotorTarget
    {
        private const int DefaultTimeout = 1000;

        int? _panTarget;
        int? _tiltTarget;
        KeeponSideTarget? _sideTarget;
        KeeponPonTarget? _ponTarget;

        int _timeout;

        public int? PanTarget { get { return _panTarget; } }
        public int? TiltTarget { get { return _tiltTarget; } }
        public KeeponSideTarget? SideTarget { get { return _sideTarget; } }
        public KeeponPonTarget? PonTarget { get { return _ponTarget; } }

        public KeeponMotorState PanState { get; set; }
        public KeeponMotorState TiltState { get; set; }
        public KeeponMotorState SideState { get; set; }
        public KeeponMotorState PonState { get; set; }

        public int? PanSpeed {get; set; }
        public int? TiltSpeed { get; set; }
        public int? PonSideSpeed { get; set; }

        public int Timeout {
            get
            {
                if (_timeout == 0) return DefaultTimeout;
                else return _timeout;
            }
            set
            {
                _timeout = value;
            }
        } // in ms
        public DateTime Started { get; set; }



        #region constructors

        public KeeponMotorTarget(int? panTarget, int? tiltTarget, KeeponSideTarget? sideTarget, int timeout)
            : this(panTarget, tiltTarget, timeout)
        {
            _sideTarget = sideTarget;
        }

        public KeeponMotorTarget(int? panTarget, int? panSpeed, int? tiltTarget, int? tiltSpeed, KeeponSideTarget? sideTarget, int? sideSpeed, int timeout)
            : this(panTarget, panSpeed, tiltTarget, tiltSpeed, timeout)
        {
            _sideTarget = sideTarget;
            PonSideSpeed = sideSpeed;
        }

        public KeeponMotorTarget(int? panTarget, int? tiltTarget, KeeponSideTarget? sideTarget) : this(panTarget, tiltTarget)
        {
            _sideTarget = sideTarget;
        }

        public KeeponMotorTarget(int? panTarget, int? panSpeed, int? tiltTarget, int? tiltSpeed, KeeponSideTarget? sideTarget, int? sideSpeed)
            : this(panTarget, panSpeed, tiltTarget, tiltSpeed)
        {
            _sideTarget = sideTarget;
            PonSideSpeed = sideSpeed;
        }





        public KeeponMotorTarget(int? panTarget, int? tiltTarget, KeeponPonTarget? ponTarget, int timeout)
            : this(panTarget, tiltTarget, timeout)
        {
            _ponTarget = ponTarget;
        }

        public KeeponMotorTarget(int? panTarget, int? panSpeed, int? tiltTarget, int? tiltSpeed, KeeponPonTarget? ponTarget, int? ponSpeed, int timeout)
            : this(panTarget, panSpeed, tiltTarget, tiltSpeed, timeout)
        {
            _ponTarget = ponTarget;
            PonSideSpeed = ponSpeed;
        }



        public KeeponMotorTarget(int? panTarget, int? tiltTarget, KeeponPonTarget? ponTarget) : this(panTarget, tiltTarget)
        {
            _ponTarget = ponTarget;
        }

        public KeeponMotorTarget(int? panTarget, int? panSpeed, int? tiltTarget, int? tiltSpeed, KeeponPonTarget? ponTarget, int? ponSpeed)
            : this(panTarget, panSpeed, tiltTarget, tiltSpeed)
        {
            _ponTarget = ponTarget;
            PonSideSpeed = ponSpeed;
        }



        public KeeponMotorTarget(int? panTarget, int? panSpeed, int? tiltTarget, int? tiltSpeed, int timeout)
            : this(panTarget, tiltTarget, timeout)
        {
            PanSpeed = panSpeed;
            TiltSpeed = tiltSpeed;
        }

        public KeeponMotorTarget(int? panTarget, int? panSpeed, int? tiltTarget, int? tiltSpeed)
            : this(panTarget, tiltTarget)
        {
            PanSpeed = panSpeed;
            TiltSpeed = tiltSpeed;
        }

        public KeeponMotorTarget(int? panTarget, int? tiltTarget)
            : this(panTarget, tiltTarget, DefaultTimeout)
        {

        }


        public KeeponMotorTarget(int? panTarget, int? tiltTarget, int timeout)
        {
            _panTarget = panTarget;
            _tiltTarget = tiltTarget;
            Timeout = timeout;
        }

        #endregion constructors





        public bool Complete
        {
            get
            {
                if (TimedOut) return true;
                if ( (_panTarget == null || PanState == KeeponMotorState.Finished || PanState == KeeponMotorState.Stalled)
                    && (_tiltTarget == null || TiltState == KeeponMotorState.Finished || TiltState == KeeponMotorState.Stalled)
                    && (_sideTarget == null || SideState == KeeponMotorState.Finished || SideState == KeeponMotorState.Stalled)
                    && (_ponTarget == null || PonState == KeeponMotorState.Finished || PonState == KeeponMotorState.Stalled))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CompletedSuccessfully
        {
            get
            {
                if (!TimedOut
                    &&(_panTarget == null || PanState == KeeponMotorState.Finished)
                    && (_tiltTarget == null || TiltState == KeeponMotorState.Finished)
                    && (_sideTarget == null || SideState == KeeponMotorState.Finished)
                    && (_ponTarget == null || PonState == KeeponMotorState.Finished))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TimedOut
        {
            get
            {
                if (Started == null) return false;

                if (DateTime.Now >= (Started.AddMilliseconds(Timeout))) return true;
                else return false;
            }
        }




    }
}
