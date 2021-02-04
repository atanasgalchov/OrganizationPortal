var ASG = (function () {  
    var Obj = {      
        Ajax: {
            get: function _ajaxGet(settings) {
                settings.method = 'GET';
                Obj.Ajax.ajax(settings);  
            },
            post: function _ajaxPost(settings) {
                settings.method = 'POST';
                Obj.Ajax.ajax(settings);          
			},
			getJson: function (settings) {
				settings.method = 'GET';
				settings.contentType = 'application/json';
				settings.dataType = 'json';

				Obj.Ajax.ajax(settings);  
			},
			postJson: function (settings) {
				settings.method = 'POST';
				settings.contentType = 'application/json';
				settings.dataType = 'json';

				Obj.Ajax.ajax(settings);  
			},
            ajax: function (settings) {
				// start loading 
				Obj.Html.showLoadingMask();

                var success = settings.success;
                settings.success = function (response) {

                    // done loading
					Obj.Html.hideLoadingMask();

                    if (!response.isSuccess) {
                        Obj.Dialog.showMessageBox(response.message, response.typeString);
                        return;
                    }
                    if (success) {
                        success(response);
                    }
                };
                var error = settings.error;
                settings.error = function (response) {

                    // done loading
					Obj.Html.hideLoadingMask();

                    if (error) {
                        error(response);
                    }
				};

                $.ajax(settings.url, settings);
            }
        },
        Dialog: {
            showMessageBox: function (message, title) {
                var settings = {
                    buttons: [
                        {
                            text: "Ok",
                            click: function () {
                                $(this).dialog("close");
                            }
                        }
                    ]
                };

                Obj.Dialog.showDialog(message, title, settings);
            },
            showDialog: function (message, title, settings) {
                var $dialog = $('<\div>');
                $dialog.attr('title', title ? title : 'Message');
                $dialog.html(message);

                $dialog.dialog(settings);
            }
		},
		Html: {
			showLoadingMask: function () {
				NProgress.start();
				$('.lmask').show();
				$('body').css({ 'overflow': 'hidden' });
			},
			hideLoadingMask: function () {
				NProgress.done();
				$('.lmask').hide();
				$('body').css({ 'overflow': 'visible' });
			}
		}
    };

    return Obj;
})();