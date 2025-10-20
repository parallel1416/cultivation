using UnityEngine;
using UnityEngine.UI;

public class DialogueTestButton : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(TestAllDialogues);
        }
    }

    private void TestAllDialogues()
    {
        // 加载Resources/Dialogue目录下的所有JSON文件
        TextAsset[] dialogueFiles = Resources.LoadAll<TextAsset>("Dialogues");

        if (dialogueFiles.Length == 0)
        {
            Debug.LogError("在Resources/Dialogues目录下未找到任何对话文件");
            return;
        }

        // 将所有对话文件加入队列
        foreach (TextAsset file in dialogueFiles)
        {
            string eventId = file.name; // 使用文件名作为事件ID
            DialogueManager.Instance.EnqueueDialogueEvent(eventId);
        }

        Debug.Log($"已加载 {dialogueFiles.Length} 个对话事件");

        // 开始播放所有对话
        DialogueManager.Instance.StartDialoguePlayback();
    }
}