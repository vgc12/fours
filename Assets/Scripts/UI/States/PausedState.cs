using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.States
{
    public sealed class PausedState : UIBaseState
    {

        public PausedState(GameObject pausedMenu, UIManager uiManager) : base(pausedMenu, uiManager) { }

    }
}