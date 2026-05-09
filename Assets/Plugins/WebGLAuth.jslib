mergeInto(LibraryManager.library, {
  OpenOAuthPopup: function (url) {
    var urlStr = UTF8ToString(url);

    var popup = window.open(urlStr, "OAuthLogin", "width=500,height=600");
    
    if (!popup) {
      alert("팝업이 차단되었습니다. 설정에서 팝업을 허용해 주세요.");
      return;
    }

    window.addEventListener("message", function(event) {
      if (event.data && event.data.type === "OAUTH_CODE") {
        if (window.unityInstance) {
          var message = event.data.provider + ":" + event.data.code;

          window.unityInstance.SendMessage("AuthSystem", "OnReceiveCodeFromJS", message);
        } else {
          console.error("Unity Instance를 찾을 수 없습니다.");
        }
      }
    }, { once: true });
  }
});