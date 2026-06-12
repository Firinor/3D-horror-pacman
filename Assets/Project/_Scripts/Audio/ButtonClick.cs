using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
    private Toggle toggle;
    private Button button;
    
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.3f);
        
        toggle = GetComponent<Toggle>();

        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(v => OnClickSound());
            yield break;
        }

        button = GetComponent<Button>();
        
        if (button != null)
        {
            button.onClick.AddListener(OnClickSound);
        }
    }

    public void OnClickSound()
    {
        if(!enabled)
            return;
        
        if(SoundManager.Instance == null)
            return;
        
        //Debug.Log(name);
        SoundManager.Instance.PlayButtonClick();
    }

    private void OnDestroy()
    {
        toggle?.onValueChanged.RemoveAllListeners();
        button?.onClick.RemoveAllListeners();
    }
}
