/* 
*   NatSuite Examples
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Components.UI {

	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using UnityEngine.EventSystems;

	[RequireComponent(typeof(EventTrigger))]
	public class RecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

		[Header(@"UI")]
		public Image button;
		public Image countdown;

		[Header(@"Behaviour"), Range(5, 15)]
		public float maxRecordingTime = 10f;

		[Header(@"Event Triggers")]
		public UnityEvent onTouchDown;
		public UnityEvent onTouchUp;
		
		private bool pressed;
		private const float MaxRecordingTime = 10f; // seconds

		void Start () => Reset();

		void Reset () {
			if (button)
				button.fillAmount = 1.0f;
			if (countdown)
				countdown.fillAmount = 0.0f;
		}

		async void IPointerDownHandler.OnPointerDown (PointerEventData eventData) {
			// Check for false touch
			pressed = true;
			await Task.Delay(200);
			if (!pressed)
				return;
			onTouchDown?.Invoke();
			// Countdown
			float startTime = Time.time, ratio = 0f;
			while (pressed && (ratio = (Time.time - startTime) / MaxRecordingTime) < 1.0f) {
				countdown.fillAmount = ratio;
				button.fillAmount = 1f - ratio;
				await Task.Delay(1);
			}
			// Reset
			Reset();
			// Stop recording
			onTouchUp?.Invoke();
		}

		void IPointerUpHandler.OnPointerUp (PointerEventData eventData) => pressed = false;
	}
}