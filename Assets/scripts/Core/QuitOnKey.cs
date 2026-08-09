using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ShellingOut
{
    /// Quits the game when the quit key (default Q) is pressed, saving
    /// first. In the editor it stops play mode instead (Application.Quit is
    /// a no-op there). Does nothing useful on WebGL -- a page can't close
    /// its own tab.
    public class QuitOnKey : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [Tooltip("Key that saves and quits the game.")]
        [SerializeField] Key quitKey = Key.Q;
#endif

        void Update()
        {
            if (!QuitPressed()) return;

            var gm = GameManager.Instance;
            if (gm != null) gm.SaveNow();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        bool QuitPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current[quitKey].wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Q);
#endif
        }
    }
}
