using System;
using System.Collections;
using UnityEngine;
using Google.Play.Review;

namespace Agraris.Tools
{
    public class IAReview : MonoBehaviour
    {
        static public IAReview instance { get { return s_Instance; } }
        static protected IAReview s_Instance;

        ReviewManager _reviewManager;
        PlayReviewInfo _playReviewInfo;

        int m_PlayFrequency = 2;
        int m_DayFrequency = 2;
        DateTime m_LastShowDate;
        static int m_CurrentPlayFrequency;

        bool reviewWasShownLately => (m_LastShowDate.AddDays(m_DayFrequency) > DateTime.Today);
        bool canRequestReview
        {
            get
            {
                if (reviewWasShownLately)
                    return false;

                if (m_CurrentPlayFrequency > m_PlayFrequency)
                    return true;

                return false;
            }
        }
        bool m_Success = false;

        public bool enableLog { get; set; }

        void Awake()
        {
            if (s_Instance == null)
            {
                gameObject.name = "IAReview";
                DontDestroyOnLoad(gameObject);

                s_Instance = this;
            }
            else
            {
                if (this != s_Instance)
                    Destroy(this.gameObject);
            }
        }

        public static void InitReview()
        {
            var type = typeof(IAReview);
            var mgr = new GameObject("IAReview", type).GetComponent<IAReview>();
        }

        public void ShowReview(int playFrequency, int dayFrequency, ref DateTime lastShowDate)
        {
            m_PlayFrequency = playFrequency;
            m_DayFrequency = dayFrequency;
            m_LastShowDate = lastShowDate;

            m_CurrentPlayFrequency += 1;

            if (!canRequestReview)
            {
                if (enableLog)
                    Debug.LogFormat("Today is {0}. You cannot Request Review until {1}.", DateTime.Today, lastShowDate.AddDays(dayFrequency));

                return;
            }

            StartCoroutine(RequestReviewObject());

            if (m_Success)
                lastShowDate = DateTime.Today;
        }

        IEnumerator RequestReviewObject()
        {
            if (enableLog)
                Debug.Log("Requesting Review Object");

#if UNITY_EDITOR
            StartCoroutine(ShowReviewObject());
#endif

            _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (enableLog)
                    Debug.LogError("Requesting Review Object Error" + requestFlowOperation.Error.ToString());

                yield break;
            }

            if (enableLog)
                Debug.Log("Requesting Review Object Success");
            _playReviewInfo = requestFlowOperation.GetResult();

            StartCoroutine(ShowReviewObject());
        }

        IEnumerator ShowReviewObject()
        {
            if (enableLog)
                Debug.Log("Showing Review Dialog");

#if UNITY_EDITOR
            m_Success = true;
            m_CurrentPlayFrequency = 0;
#endif

            if (_reviewManager == null)
                yield break;

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;
            _playReviewInfo = null; // Reset the object
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (enableLog)
                    Debug.LogError("Showing Review Object Error. " + launchFlowOperation.Error.ToString());

                m_Success = false;
                yield break;
            }
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
            if (enableLog)
                Debug.Log("Showing Review Object Success");

            m_Success = true;
            m_CurrentPlayFrequency = 0;
        }
    }
}