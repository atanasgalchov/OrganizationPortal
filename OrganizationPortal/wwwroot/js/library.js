(function($){ "use strict";

/* -----------------------------------------------------------------------------

	PLUGINS

----------------------------------------------------------------------------- */

	/* -------------------------------------------------------------------------
		ACCORDION
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrAccordion ) {
		$.fn.lsvrAccordion = function(){

			var $this = $(this),
				isToggle = $this.hasClass( 'm-toggle' ) ? true : false,
				items = $this.find( '.accordion-items > li' );

			items.filter( '.m-active' ).find( '.accordion-content' ).slideDown( 300 );

			$this.find( '.accordion-title' ).click(function(){
				if ( ! $(this).parent().hasClass( 'm-active' ) ) {
					if ( ! isToggle ) {
						items.filter( '.m-active' ).find( '.accordion-content' ).slideUp(300);
						items.filter( '.m-active' ).removeClass( 'm-active' );
					}
					$(this).parent().find( '.accordion-content' ).slideDown(300);
					$(this).parent().addClass( 'm-active' );
				}
				else {
					$(this).parent().find( '.accordion-content' ).slideUp(300);
					$(this).parent().removeClass( 'm-active' );
				}
			});

		};
	}

	/* -------------------------------------------------------------------------
		AJAX FORM
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrAjaxForm ) {
		$.fn.lsvrAjaxForm = function(){
		// REQUIRED PLUGINS
		if ( $.fn.lsvrIsFormValid ) {

			var form = $(this),
				submitBtn = form.find( '.submit-btn' );

			form.submit(function(e){
				e.preventDefault();

				if ( ! submitBtn.hasClass( 'm-loading' ) ) {

					// CLEAN OLD MESSAGES
					form.find( '.c-alert-message.m-success, .c-alert-message.m-phpvalidation-error' ).slideUp( 300, function(){
						$(this).remove();
					});

					// FORM NOT VALID
					if ( ! form.lsvrIsFormValid() ) {
						form.find( 'p.c-alert-message.m-warning.m-validation-error' ).slideDown(300);
						return false;
					}
					// FORM VALID
					else {

						submitBtn.addClass( 'm-loading' ).attr( 'data-label', submitBtn.text() ).text( submitBtn.data( 'loading-label' ) );

						// AJAX REQUEST
						$.ajax({
							type: 'POST',
							url: form.attr( 'action' ),
							data: form.serialize(),
							success: function( data ){

								form.find( '.c-alert-message.m-validation-error' ).hide();
								form.prepend( data );
								form.find( '.c-alert-message.m-success, .c-alert-message.m-phpvalidation-error' ).slideDown(300);
								submitBtn.removeClass( 'm-loading' ).text( submitBtn.attr( 'data-label' ) );

								// RESET ALL INPUTS
								if ( data.indexOf( 'success' ) > 0 ) {
									form.find( 'input, textarea' ).each( function() {
										$(this).val( '' );
									});
								}

							},
							error: function(){
								form.find( '.c-alert-message.m-validation-error' ).slideUp(300);
								form.find( '.c-alert-message.m-request-error' ).slideDown(300);
								submitBtn.removeClass( 'm-loading' ).text( submitBtn.attr( 'data-label' ) );
							}
						});

					}

				}
			});

		}};
	}

	/* -------------------------------------------------------------------------
		ALERT MESSAGE
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrAlertMessage ) {
		$.fn.lsvrAlertMessage = function(){

			var $this = $(this);

			$this.find( '.alert-close' ).click(function(){
				$this.slideUp( 300 );
			});

		};
	}

	/* -------------------------------------------------------------------------
		FIELD VALIDATION
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrIsFieldValid ) {
		$.fn.lsvrIsFieldValid = function(){

			var field = $(this),
				value = field.val(),
				placeholder = field.attr( 'placeholder' ) ? field.attr( 'placeholder' ) : false,
				valid = false,
				emailValid = function( email ) {
					var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
					return re.test(email);
				};

			if ( value.trim() !== '' && ! ( placeholder && value === placeholder ) ) {

				// EMAIL FIELDS
				if ( field.attr( 'type' ) === 'email' ) {
					if ( ! emailValid( value ) ) {
						field.addClass( 'm-error' );
					}
					else {
						field.removeClass( 'm-error' );
						valid = true;
					}
				}

				// SELECT FIELD
				else if ( field.prop( 'tagName' ).toLowerCase() === 'select' ) {
					if ( value === null ) {
						field.addClass( 'm-error' );
					}
					else {
						field.removeClass( 'm-error' );
						valid = true;
					}
				}

				// DEFAULT FIELD
				else {
					field.removeClass( 'm-error' );
					valid = true;
				}

			}
			else {

				field.addClass( 'm-error' );

				// REVALIDATE ON CHANGE
				field.change(function(){
					field.lsvrIsFieldValid();
				});

			}

			return valid;

		};
	}

	/* -------------------------------------------------------------------------
		FLUID MEDIA
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrFluidEmbedMedia ) {
		$.fn.lsvrFluidEmbedMedia = function( ){

			var $this = $(this),
				allMedia;

			// CHECK FOR ANY LOOSE IFRAMES
			$this.find( 'iframe, embed' ).each(function(){

				if ( $(this).parents( '.embed-media' ).length < 1 ) {
					if ( $(this).parent().is( 'p' ) ) {
						$(this).unwrap();
					}
					$(this).wrap( '<div class="embed-media"></div>' );
				}

			});

			var reloadFluidMedia = function(){
				// Resize all media according to their own aspect ratio
				allMedia.each(function() {
					var el = $(this),
						elContainer = el.parents( '.embed-media' ),
						newWidth = elContainer.width();
					el.width( newWidth ).height( newWidth * el.attr( 'data-aspectratio' ) );
				});
				$.event.trigger({
					type: 'fluidMediaReloaded',
					message: 'Fluid media reloaded.',
					time: new Date()
				});
			};

			var generateFluidMedia = function(){
				// Find all media
				allMedia = $this.find( '.embed-media iframe, .embed-media embed' );
				// The element that is fluid width
				//$fluidEl = $('.embed-media').first();
				// Figure out and save aspect ratio for each media
				allMedia.each(function() {
					$(this).attr( 'data-aspectratio', $(this).height() / $(this).width() )
						.removeAttr( 'height' )
						.removeAttr( 'width' );
				});
				reloadFluidMedia();
			};

			if ( $this.find( '.embed-media' ).length > 0 ) {
				generateFluidMedia();
				$(window).resize(function(){
					reloadFluidMedia();
				});
			}

		};
	}

	/* -------------------------------------------------------------------------
		FORM VALIDATION
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrIsFormValid ) {
		$.fn.lsvrIsFormValid = function() {
		// REQUIRED PLUGINS
		if ( $.fn.lsvrIsFieldValid ) {

			// TRIM FIX FOR IE
			if ( typeof String.prototype.trim !== 'function' ) {
				String.prototype.trim = function() {
					return this.replace(/^\s+|\s+$/g, '');
				};
			}

			var form = $(this),
			formValid = true;

			// CHECK REQUIRED FIELDS
			form.find( '*[required="required"]' ).each(function(){
				formValid = ! $(this).lsvrIsFieldValid() ? false : formValid;
			});

			// CHECK REQUIRED ONE FIELDS
			var requireOneValid = false;
			form.find( 'input.m-required-one, textarea.m-required-one, select.m-required-one' ).each(function(){
				if ( $(this).lsvrIsFieldValid() ) {
					requireOneValid = true;
					form.find( 'input.m-required-one, textarea.m-required-one, select.m-required-one' ).removeClass( 'm-error' );
				}
			});
			if ( form.find( '.m-require-one' ).length > 0 && ! requireOneValid ) {
				formValid = false;
			}
			if ( formValid ) {
				form.find( 'input.m-required-one, textarea.m-required-one, select.m-required-one' ).removeClass( 'm-error' );
			}

			form.find( '.m-error' ).first().focus();

			return formValid;

		}};
	}

	/* -------------------------------------------------------------------------
		GOOGLE MAP
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrGoogleMap ) {
		$.fn.lsvrGoogleMap = function( latitude, longitude ) {

			var $this = $(this),
				zoom = $this.data( 'zoom' ) ? $this.data( 'zoom' ) : 10,
				enableMouseWheel = $this.data( 'enable-mousewheel' ) && String( $this.data( 'enable-mousewheel' ) ) === 'true' ? true : false,
				mapType = $this.data( 'maptype' ) ? $this.data( 'maptype' ) : 'SATELLITE',
				latLng = new google.maps.LatLng( latitude, longitude ),
				elementId = $this.attr( 'id' );

			switch ( mapType ) {
				case 'roadmap':
					mapType = google.maps.MapTypeId.ROADMAP;
					break;
				case 'terrain':
					mapType = google.maps.MapTypeId.TERRAIN;
					break;
				case 'hybrid':
					mapType = google.maps.MapTypeId.HYBRID;
					break;
				default:
					mapType = google.maps.MapTypeId.SATELLITE;
			}

			var mapOptions = {
				center: latLng,
				zoom: zoom,
				mapTypeId: mapType,
				scrollwheel: enableMouseWheel,
			};

			var map = new google.maps.Map(document.getElementById( elementId ),
				mapOptions );

			var marker = new google.maps.Marker({
				position: latLng,
				map: map
			});

		};
	}

	if ( ! $.fn.lsvrGoogleMapsLoaded ) {
		$.fn.lsvrGoogleMapsLoaded = function (element) {
			
			if ( typeof element == 'undefined' ) {
				element = 'body';
			}
			$( element + ' .gmap-canvas' ).each(function(){

				// OPTIONS
				var $this = $(this),
					latitude = $this.data( 'latitude' ) && $this.data( 'latitude' ) !== '' ? $this.data( 'latitude' ) : false,
					longitude = $this.data( 'longitude' ) && $this.data( 'longitude' ) !== '' ? $this.data( 'longitude' ) : false,
					address = $this.data( 'address' ) && $this.data( 'address' ) !== '' ? $this.data( 'address' ) : false,
					elementIndex = $this.index( '.gmap-canvas' );

				// ADD UNIQUE ID
				$this.attr( 'id', 'google-map-' + elementIndex );

				// GET LATITUDE AND LONGITUDE FROM ADDRESS
				if ( address ) {

					var geocoder = new google.maps.Geocoder();
					geocoder.geocode( { 'address': address }, function( results, status ) {
						if ( status == google.maps.GeocoderStatus.OK ) {
							latitude = results[0].geometry.location.lat();
							longitude = results[0].geometry.location.lng();
							$this.lsvrGoogleMap( latitude, longitude );
						}
						else if ( latitude && longitude ) {
							$this.lsvrGoogleMap( latitude, longitude );
						}
					});

				}
				// OR USE LATITUDE & LANGITUDE FROM ATTRIBUTES
				else if ( latitude && longitude ) {
					$this.lsvrGoogleMap( latitude, longitude );
				}

			});
		};
	}

	if ( ! $.fn.lsvrLoadGoogleMaps ) {
		// REQUIRED PLUGINS
		if ( $.fn.lsvrGoogleMapsLoaded && $.fn.lsvrGoogleMap ) {
			$.fn.lsvrLoadGoogleMaps = function() {

				// GET API KEY
				var apiKey = $( '.gmap-canvas[data-google-api-key]' ).first().length > 0 ? $( '.gmap-canvas' ).attr( 'data-google-api-key' ) : false;
				if ( apiKey !== false ) {

					// INSERT GOOGLE API JS
					if ( $( 'script.googleMapsApiScript' ).length < 1 ) {
						var script = document.createElement( 'script' );
						script.className = 'googleMapsApiScript';
						script.type = 'text/javascript';
						script.src = 'https://maps.googleapis.com/maps/api/js?key=' + apiKey + '&callback=jQuery.fn.lsvrGoogleMapsLoaded';
						document.body.appendChild( script );
					}
					else {
						$.fn.lsvrGoogleMapsLoaded();
					}

				}

			};
		}
	}

	/* -------------------------------------------------------------------------
		IMAGES LOADED
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrImagesLoaded ) {
		$.fn.lsvrImagesLoaded = function( func ) {
			if ( $.isFunction( func ) ) {

				var images = $(this).find( 'img' ),
				loadedImages = 0,
				count = images.length;

				if ( count > 0 ) {
					images.one( 'load', function(){
						loadedImages++;
						if ( loadedImages === count ){
							func.call();
						}
					}).each(function() {
						if ( this.complete ) { $(this).load(); }
					});
				}
				else {
					func.call();
				}

			}
		};
	}

	/* -------------------------------------------------------------------------
		LIGHTBOX
	------------------------------------------------------------------------- */

	// LIGHTBOX SETUP
	if ( ! $.fn.magnificPopupSetup ) {
		$.fn.magnificPopupSetup = function(){
		// REQUIRED PLUGINS
		if ( $.fn.magnificPopup ) {

			$.extend( true, $.magnificPopup.defaults, {
				tClose: $( '.mp-labels' ).attr( 'data-mp-tClose' ),
				tLoading: $( '.mp-labels' ).attr( 'data-mp-tLoading' ),
				gallery: {
					tPrev: $( '.mp-labels' ).attr( 'data-mp-tPrev' ),
					tNext: $( '.mp-labels' ).attr( 'data-mp-tNext' ),
					tCounter: '%curr% / %total%'
				},
				image: {
					tError: $( '.mp-labels' ).attr( 'data-mp-image-tError' ),
				},
				ajax: {
					tError: $( '.mp-labels' ).attr( 'data-mp-ajax-tError' ),
				}
			});

		}};
	}
	if ( $.fn.magnificPopupSetup ) {
		$.fn.magnificPopupSetup();
	}

	if ( ! $.fn.lsvrInitLightboxes ) {
		$.fn.lsvrInitLightboxes = function(){
		// REQUIRED PLUGINS
		if ( $.fn.magnificPopup ) {
			
			// IMAGES
			$(this).find( 'a.lightbox' ).magnificPopup({
				type: 'image',
				removalDelay: 300,
				mainClass: 'mfp-fade',
				gallery: {
					enabled: true
				}
			});

			// VIDEOS
			$(this).find( 'a.lightbox-video' ).magnificPopup({
				type: 'iframe',
				removalDelay: 300,
				mainClass: 'mfp-fade',
				gallery: {
					enabled: true
				}
			});

		}};
	}

	/* -------------------------------------------------------------------------
		LOAD HIRES IMAGES
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrLoadHiresImages ) {
		$.fn.lsvrLoadHiresImages = function() {
			if ( window.devicePixelRatio > 1 ) {
				$(this).find( 'img[data-hires]' ).each(function(){
					if ( $(this).data( 'hires' ) !== '' ) {
						$(this).attr( 'src', $(this).data( 'hires' ) );
					}
				});
			}
		};
	}

	/* -------------------------------------------------------------------------
		MAILCHIMP SUBSCRIBE FORM
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrMailchimpSubscribeForm ) {
		$.fn.lsvrMailchimpSubscribeForm = function(){
		// REQUIRED PLUGINS
		if ( $.fn.lsvrIsFormValid ) {

			var form = $(this).find( 'form' ),
				submitBtn = form.find( '.submit-btn' );

			form.submit(function(e){
				e.preventDefault();
				if ( ! form.hasClass( 'm-loading' ) ) {

					// FORM IS VALID
					if ( form.lsvrIsFormValid() ) {

						form.find( 'p.c-alert-message.m-warning.m-validation-error' ).slideUp(300);
						form.addClass( 'm-loading' );
						//submitBtn.attr( 'data-label', submitBtn.text() ).text( submitBtn.data( 'loading-label' ) );

						// SEND AJAX REQUEST
						$.ajax({
							type: form.attr( 'method' ),
							url: form.attr( 'action' ),
							data: form.serialize(),
							cache : false,
							dataType : 'json',
							contentType: "application/json; charset=utf-8",
							// WAIT FOR RESPONSE
							success: function( data ){

								if ( data.result === 'success' ) {
									form.find( '.c-alert-message' ).hide();
									form.find( '.c-alert-message.m-success' ).append( '<br>' + data.msg ).slideDown(300);
									form.find( '.form-fields' ).slideUp(300);
								}
								else {
									form.find( '.c-alert-message.m-validation-error' ).slideUp(300);
									form.find( '.c-alert-message.m-request-error span' ).text( data.msg );
									form.find( '.c-alert-message.m-request-error' ).slideDown(300);
								}

								form.removeClass( 'm-loading' );
								//submitBtn.text( submitBtn.attr( 'data-label' ) );

							},
							error: function( data ){

								form.attr( 'data-error-msg', data.msg );
								form.find( '.m-alert-message.m-validation-error' ).slideUp(300);
								form.find( '.m-alert-message.m-request-error' ).slideDown(300);
								form.removeClass( 'loading' );
								//submitBtn.text( submitBtn.attr( 'data-label' ) );

							}
						});

					}

					//  FORM IS INVALID
					else {
						form.find( 'p.c-alert-message.m-warning.m-validation-error' ).slideDown(300);
						return false;
					}

				}
			});

		}};
	}

	/* -------------------------------------------------------------------------
		MEDIA QUERY BREAKPOINT
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrGetMediaQueryBreakpoint ) {
		$.fn.lsvrGetMediaQueryBreakpoint = function() {

			if ( $( '#media-query-breakpoint' ).length < 1 ) {
				$( 'body' ).append( '<span id="media-query-breakpoint" style="display: none;"></span>' );
			}
			var value = $( '#media-query-breakpoint' ).css( 'font-family' );
			if ( typeof value !== 'undefined' ) {
				value = value.replace( "\"", "" ).replace( "\"", "" ).replace( "\'", "" ).replace( "\'", "" );
			}
			if ( isNaN( value ) ) {
				return $(window).width();
			}
			else {
				return parseInt( value );
			}

		};
	}

	/* -------------------------------------------------------------------------
		SLIDER
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrSlider ) {
		$.fn.lsvrSlider = function(){
		// REQUIRED PLUGINS
		if ( $.fn.owlCarousel ) {

			var slider = $(this),
				slideList = slider.find( '.slide-list' ),
				slideCount = slideList.find( '> .slide' ).length,
				slides = slideList.find( '> .slide' ),
				autoplay = slider.data( 'autoplay' ) && parseInt( slider.data( 'autoplay' ) ) > 0 ? true : false,
				autoplayTimeout = slider.data( 'autoplay' ) && parseInt( slider.data( 'autoplay' ) ) > 0 ? parseInt( slider.data( 'autoplay' ) ) : 0,
				rtl = $( 'html' ).attr( 'dir' ) && $( 'html' ).attr( 'dir' ) == 'rtl' ? true : false;
				autoplayTimeout = autoplayTimeout * 1000;

			if ( slideCount > 1 ) {

				slideList.owlCarousel({
					rtl: rtl,
					autoHeight: true,
					loop: true,
					nav: true,
					navText: new Array( '<i class="fa fa-angle-left"></i>', '<i class="fa fa-angle-right"></i>' ),
					navRewind: true,
					dots: false,
					autoplay: autoplay,
					autoplayTimeout: autoplayTimeout,
					autoplayHoverPause: true,
					responsive:{
						0: {
							items: 1
						}
					},
					onTranslated: function(){

						// REFRESH INDICATOR
						if ( autoplay ) {
							slider.find( '.slider-indicator > span' ).stop( 0, 0 );
						}
						if ( autoplay && mediaQueryBreakpoint > 991 ) {
							slider.find( '.slider-indicator > span' ).css( 'width', 0 );
							if ( ! slider.hasClass( 'm-paused' ) ) {
								slider.find( '.slider-indicator > span' ).stop( 0, 0 ).animate({ width : "100%" }, autoplayTimeout );
							}
						}

					}
				});

				// AUTO SLIDE INDICATOR
				if ( autoplay ) {

					// CREATE
					slider.addClass( 'm-has-indicator' );
					slider.append( '<div class="slider-indicator"><span></span></div>' );

					// INITIAL ANIMATION
					slider.find( '.slider-indicator > span' ).animate({
						width : "100%"
					}, autoplayTimeout, 'linear' );

					// PAUSE
					var sliderPause = function(){
						slider.addClass( 'm-paused' );
						slider.find( '.slider-indicator > span' ).stop( 0, 0 );
					};
					var sliderResume = function(){
						slider.removeClass( 'm-paused' );
						slider.find( '.slider-indicator > span' ).stop( 0, 0 ).animate({
							width : "100%"
						}, autoplayTimeout, 'linear' );
					};

					slider.hover(function(){
						sliderPause();
					}, function(){
						sliderResume();
					});

					// STOP ON SMALLER RESOLUTIONS
					$( document ).on( 'screenTransition', function(){
						if ( mediaQueryBreakpoint <= 991 ) {
							sliderPause();
						}
					});
					if ( mediaQueryBreakpoint <= 991 ) {
						sliderPause();
					}

				}

			}

		}};
	}

	/* -------------------------------------------------------------------------
		TABS
	------------------------------------------------------------------------- */

	if ( ! $.fn.lsvrTabs ) {
		$.fn.lsvrTabs = function(){

			var $this = $(this),
				tabs = $this.find( '.tab-list > li' ),
				contents = $this.find( '.content-list > li' );

			tabs.click(function(){
				if ( ! $(this).hasClass( 'm-active' ) ) {
					var index = $(this).index();
					tabs.filter( '.m-active' ).removeClass( 'm-active' );
					$(this).addClass( 'm-active' );
					contents.filter( ':visible' ).slideUp( 300, function(){
						$(this).removeClass( 'm-active' );
					});
					contents.filter( ':eq(' + index + ')' ).slideDown(300).addClass( 'm-active' );
				}
			});

		};
	}


/* -----------------------------------------------------------------------------

	EVENTS

----------------------------------------------------------------------------- */

	/* -------------------------------------------------------------------------
		SCREEN SIZE TRANSITION
	------------------------------------------------------------------------- */

	var mediaQueryBreakpoint;
	if ( $.fn.lsvrGetMediaQueryBreakpoint ) {
		mediaQueryBreakpoint = $.fn.lsvrGetMediaQueryBreakpoint();
		$(window).resize(function(){
			if ( $.fn.lsvrGetMediaQueryBreakpoint() !== mediaQueryBreakpoint ) {
				mediaQueryBreakpoint = $.fn.lsvrGetMediaQueryBreakpoint();
				$.event.trigger({
					type: 'screenTransition',
					message: 'Screen transition completed.',
					time: new Date()
				});
			}
		});
	}
	else {
		mediaQueryBreakpoint = $(document).width();
	}

})(jQuery);