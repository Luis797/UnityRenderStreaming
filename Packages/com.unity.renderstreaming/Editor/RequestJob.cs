using UnityEditor.PackageManager.Requests;  //Request
using UnityEditor.PackageManager;           //StatusCode
using System;                               //Action

namespace Unity.RenderStreaming.Editor {

//---------------------------------------------------------------------------------------------------------------------

//Examples of T: PackageCollection(from ListRequest), PackageInfo (from AddRequest)
internal class RequestJob<T> : IRequestJob  {

    internal RequestJob(Request<T> req, Action<Request<T>> onSuccess, Action<Request<T>> onFail) {
        m_request = req;
        m_onSuccess = onSuccess;
        m_onFail = onFail;
    }

//---------------------------------------------------------------------------------------------------------------------

    public StatusCode Update() {
        if (null == m_request) {
            OnFail();          
            return StatusCode.Failure;
        }

        if (m_request.IsCompleted) {
            if (StatusCode.Success == m_request.Status ) {
                OnSuccess();          
                return StatusCode.Success;
            } else {
                OnFail();          
                return StatusCode.Failure;
            }
        }

        return StatusCode.InProgress;

    }

//---------------------------------------------------------------------------------------------------------------------


    void OnSuccess() {
        if (null==m_onSuccess)
            return;

        m_onSuccess(m_request);
    }

//---------------------------------------------------------------------------------------------------------------------

    void OnFail() {
        if (null==m_onFail)
            return;

        m_onFail(m_request);
    }

//---------------------------------------------------------------------------------------------------------------------

    Request<T> m_request;
    Action<Request<T>> m_onSuccess;
    Action<Request<T>> m_onFail;
}

} //Unity.RenderStreaming.Editor
