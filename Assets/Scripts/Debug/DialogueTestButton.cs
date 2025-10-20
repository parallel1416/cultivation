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
        // ����Resources/DialogueĿ¼�µ�����JSON�ļ�
        TextAsset[] dialogueFiles = Resources.LoadAll<TextAsset>("Dialogues");

        if (dialogueFiles.Length == 0)
        {
            Debug.LogError("��Resources/DialoguesĿ¼��δ�ҵ��κζԻ��ļ�");
            return;
        }

        // �����жԻ��ļ��������
        foreach (TextAsset file in dialogueFiles)
        {
            string eventId = file.name; // ʹ���ļ�����Ϊ�¼�ID
            DialogueManager.Instance.EnqueueDialogueEvent(eventId);
        }

        Debug.Log($"�Ѽ��� {dialogueFiles.Length} ���Ի��¼�");

        // ��ʼ�������жԻ�
        DialogueManager.Instance.StartDialoguePlayback();
    }
}