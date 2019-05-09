using System;
using System.Collections.Generic;
using ConnectApp.canvas;
using ConnectApp.components;
using ConnectApp.constants;
using ConnectApp.utils;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.screens
{ 
    public class WebViewScreen : StatefulWidget
    {
        public WebViewScreen(
            string url = null ,
            Key key = null
        ) : base(key)
        {
            this.url = url;
        }

        public readonly string url;

        public override State createState()
        {
            return new _WebViewScreenState();
        }
    }
    public class _WebViewScreenState : State<WebViewScreen>
    {
        private WebViewObject _webViewObject = null;
        private float _progress;
        private bool _onClose;
        private Timer _timer;

        public override void initState()
        {
            base.initState();
            if (!Application.isEditor)
            {
                _webViewObject = WebViewManager.instance.webViewObject; 
                _webViewObject.Init(
                    ua: "", 
                    enableWKWebView: true, 
                    transparent: true
                );
                _webViewObject.LoadURL(widget.url);
                _webViewObject.ClearCookies();
                if (HttpManager.getCookie().isNotEmpty()) {
#if UNITY_IOS
                    _webViewObject.AddCustomHeader("Cookie", HttpManager.getCookie());
#endif
                }
                _webViewObject.SetVisibility(true);
            }
            _progress = 0;
            _onClose = false;
            _timer = Window.instance.run(new TimeSpan(0,0,0,0,200), () => {
                if (_progress < 1) {
                    _progress += 0.1f;
                    setState(() => {});
                }
                else {
                    _timer.cancel();
                }
            }, true);
        }
        
        public override void dispose() {
            _timer.cancel();
            _timer.Dispose();
            if (!Application.isEditor) {
                _webViewObject.SetVisibility(false);
            }
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            Widget progressWidget = new Container();
            var progressHeight = 0;
            if (_progress < 1.0f) {
                progressWidget = new CustomProgress(
                    _progress,
                    CColors.Transparent
                );
                progressHeight = (int) (2 * Window.instance.devicePixelRatio);
            }
            if (!Application.isEditor)
            {
                var ratio = Window.instance.devicePixelRatio;
                var top = (int) ( 44 * ratio);
                if (Application.platform != RuntimePlatform.Android)
                {
                    top = (int) ((MediaQuery.of(context).padding.top + 44) * ratio);
                }
                if (_progress < 1.0f) {
                    top += progressHeight;
                }
                var bottom = (int) (MediaQuery.of(context).padding.bottom * ratio);
                _webViewObject.SetMargins(0, top,0, bottom);
            }
            var child = new Container(
                color: CColors.background3,
                child: new Column(
                    children: new List<Widget> {
                        _buildNavigationBar(),
                        progressWidget
                    }
                )
            );
            
            Widget closeText = new Container();
            if (_onClose) {
                closeText = new Text(
                    "正在关闭...",
                    style: CTextStyle.PXLarge
                );
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Container(
                        color: CColors.background3,
                        child: new Column(
                            children: new List<Widget> {
                                new Container(
                                    child: new Column(
                                        children: new List<Widget> {
                                            _buildNavigationBar(),
                                            progressWidget
                                        }
                                    )
                                ),
                                new Expanded(
                                    child: new Center(
                                        child: closeText
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }       
        private Widget _buildNavigationBar() {
            return new Container(
                height: 44,
                color: CColors.White,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.start,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new GestureDetector(
                            onTap: () => {
                                _onClose = true;
                                setState(() => {});
                                if (Router.navigator.canPop()) {
                                    Router.navigator.pop();
                                }
                                if (!Application.isEditor)
                                {
                                    _webViewObject.SetVisibility(false);
                                }
                            },
                            child: new Container(
                                padding: EdgeInsets.symmetric(10, 16),
                                color: CColors.Transparent,
                                child: new Icon(Icons.arrow_back, size: 24, color: CColors.icon3))
                        )
                    }
                )
            );
        }
    }
}