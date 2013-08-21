using Leap;

namespace Vyrolan.VMCS
{
    internal class HandManager : IFrameUpdate
    {
        private readonly PersistentHand[] _PotentialHands = new[] { new PersistentHand(), new PersistentHand() };
        private readonly PersistentHand _LeftHand = new PersistentHand();
        private readonly PersistentHand _RightHand = new PersistentHand();

        public PersistentHand LeftHand { get { return _LeftHand; } }
        public PersistentHand RightHand { get { return _RightHand; } }

        public bool Update(Frame frame)
        {
            _LeftHand.Update(frame);
            _RightHand.Update(frame);
            _PotentialHands[0].Update(frame);
            _PotentialHands[1].Update(frame);

            foreach (var h in frame.Hands)
            {
                // Skip all the currently-tracked hands
                if (h.Id == _LeftHand.Id
                    || h.Id == _RightHand.Id
                    || h.Id == _PotentialHands[0].Id
                    || h.Id == _PotentialHands[1].Id
                    )
                    continue;

                // New potential hand
                foreach (var ph in _PotentialHands)
                    if (ph.IsFinalized)
                    {
                        ph.Initialize(h);
                        break;
                    }
                // else we already have two potentials?  too many hands!
            }

            // Check for a new left hand
            if (_LeftHand.IsFinalized)
                for (var i = 0; i < 2; i++)
                    if (_PotentialHands[i].IsStabilized && (!_RightHand.IsFinalized || _PotentialHands[i].StabilizedHand.PalmPosition.x < 0))
                    {
                        _LeftHand.PromotePotentialHand(_PotentialHands[i]);
                        break;
                    }

            // Check for a new left hand
            if (_RightHand.IsFinalized)
                for (var i = 0; i < 2; i++)
                    if (_PotentialHands[i].IsStabilized && (!_LeftHand.IsFinalized || _PotentialHands[i].StabilizedHand.PalmPosition.x > 0))
                    {
                        _RightHand.PromotePotentialHand(_PotentialHands[i]);
                        break;
                    }

            return true;
        }

        public string Dump()
        {
            var s = "";
            if (_LeftHand.IsStabilized)
                s += "\nLeft Hand:\n------------------------------------------------------------------------\n" + _LeftHand.Dump();
            if (_RightHand.IsStabilized)
                s += "\nRight Hand:\n------------------------------------------------------------------------\n" + _RightHand.Dump();
            if (!_PotentialHands[0].IsFinalized)
                s += "\nPotential Hand 0:\n------------------------------------------------------------------------\n" + _PotentialHands[0].Dump();
            if (!_PotentialHands[1].IsFinalized)
                s += "\nPotential Hand 1:\n------------------------------------------------------------------------\n" + _PotentialHands[1].Dump();
            return s;
        }
    }
}
