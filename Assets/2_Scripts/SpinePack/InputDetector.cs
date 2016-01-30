/*****************************************************************************
 * Spine Asset Pack License
 * Version 1.0
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use the Asset Pack and derivative works only as
 * incorporated and embedded components of your software applications and to
 * distribute such software applications. Any source code contained in the Asset
 * Pack may not be distributed in source form. You may otherwise not reproduce,
 * distribute, sublicense, rent, lease or lend the Asset Pack. It is emphasized
 * that you are not entitled to distribute or transfer the Asset Pack in any way
 * other way than as integrated components of your software applications.
 * 
 * THIS ASSET PACK IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS ASSET PACK, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using UnityEngine;
using System.Collections;

public class InputDetector : MonoBehaviour {

	public GameObject keyboardInstructions;
	public GameObject gamepadInstructions;

	public GameCharacterInput input;
	public UnityEngine.UI.Toggle toggle;

	void Start () {

		if (PlayerPrefs.HasKey("UseKeyboard")) {
			int val = PlayerPrefs.GetInt("UseKeyboard", 0);
			if (val > 0)
				toggle.isOn = true;
			else
				toggle.isOn = false;
		} else {
			if (Input.GetJoystickNames().Length == 0) {
				toggle.isOn = true;
			} else {
				toggle.isOn = false;
			}
		}

		ValueChanged();
	}

	void OnDisable () {
		PlayerPrefs.SetInt("UseKeyboard", toggle.isOn ? 1 : 0);
	}

	public void ValueChanged () {
		input.useKeyboard = toggle.isOn;

		if (toggle.isOn) {
			keyboardInstructions.SetActive(true);
			gamepadInstructions.SetActive(false);
		} else {
			keyboardInstructions.SetActive(false);
			gamepadInstructions.SetActive(true);
		}

	}
}
