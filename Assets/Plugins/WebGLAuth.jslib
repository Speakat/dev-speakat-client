mergeInto(LibraryManager.library, {
  OpenOAuthPopup: function (urlPtr) {
    var url = UTF8ToString(urlPtr);

    console.log("[WebGLAuth] OpenOAuthPopup:", url);

    var popup = window.open(
      url,
      "oauthPopup",
      "width=500,height=700,scrollbars=yes,resizable=yes"
    );

    function handleOAuthMessage(event) {
      if (!event.data || event.data.type !== "OAUTH_CODE") {
        return;
      }

      var provider = event.data.provider;
      var code = event.data.code;

      if (!provider || !code) {
        console.error("[WebGLAuth] provider 또는 code가 비어 있음:", event.data);
        return;
      }

      var payload = provider + ":" + code;

      console.log("[WebGLAuth] OAuth payload received:", payload);

      try {
        /*
         * jslib 내부에서는 window.unityInstance를 못 찾는 경우가 많아서
         * SendMessage를 직접 호출하는 방식으로 처리.
         *
         * Unity Hierarchy에 있는 GameObject 이름: AuthSystem
         * AuthManager.cs에 있는 public 메서드 이름: OnReceiveCodeFromJS
         */
        SendMessage("AuthSystem", "OnReceiveCodeFromJS", payload);

        console.log("[WebGLAuth] SendMessage 성공:", payload);
      } catch (e) {
        console.error("[WebGLAuth] SendMessage 실패:", e);
      }

      window.removeEventListener("message", handleOAuthMessage);

      if (popup && !popup.closed) {
        popup.close();
      }
    }

    window.addEventListener("message", handleOAuthMessage);
  }
});