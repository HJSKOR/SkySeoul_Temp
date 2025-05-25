#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class ApplicationControl
{
    public static void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // ������ ���� ����
#else
        Application.Quit(); // ���� ���� ����
#endif
    }

}
