$(function() {
	'use strict'

	
	// ______________ PAGE LOADING
	$("#global-loader").fadeOut("slow");

	// ______________ Card
	const DIV_CARD = 'div.card';

	// ______________ Function for remove card
	$(document).on('click', '[data-bs-toggle="card-remove"]', function(e) {
		let $card = $(this).closest(DIV_CARD);
		$card.remove();
		e.preventDefault();
		return false;
	});

	// ______________ Functions for collapsed card
	$(document).on('click', '[data-bs-toggle="card-collapse"]', function(e) {
		let $card = $(this).closest(DIV_CARD);
		$card.toggleClass('card-collapsed');
		e.preventDefault();
		return false;
	});

	// ______________ Card full screen
	$(document).on('click', '[data-bs-toggle="card-fullscreen"]', function(e) {
		let $card = $(this).closest(DIV_CARD);
		$card.toggleClass('card-fullscreen').removeClass('card-collapsed');
		e.preventDefault();
		return false;
	});

	// ______________Main-navbar
	if (window.matchMedia('(min-width: 992px)').matches) {
		$('.main-navbar .active').removeClass('show');
		$('.main-header-menu .active').removeClass('show');
	}
	$('.main-header .dropdown > a').on('click', function(e) {
		e.preventDefault();
		$(this).parent().toggleClass('show');
		$(this).parent().siblings().removeClass('show');
	});
	$('.mobile-main-header .dropdown > a').on('click', function(e) {
		e.preventDefault();
		$(this).parent().toggleClass('show');
		$(this).parent().siblings().removeClass('show');
	});
	$('.main-navbar .with-sub').on('click', function(e) {
		e.preventDefault();
		$(this).parent().toggleClass('show');
		$(this).parent().siblings().removeClass('show');
	});
	$('.dropdown-menu .main-header-arrow').on('click', function(e) {
		e.preventDefault();
		$(this).closest('.dropdown').removeClass('show');
	});
	$('#mainNavShow').on('click', function(e) {
		e.preventDefault();
		$('body').toggleClass('main-navbar-show');
	});
	$('#mainContentLeftShow').on('click touch', function(e) {
		e.preventDefault();
		$('body').addClass('main-content-left-show');
	});
	$('#mainContentLeftHide').on('click touch', function(e) {
		e.preventDefault();
		$('body').removeClass('main-content-left-show');
	});
	$('#mainContentBodyHide').on('click touch', function(e) {
		e.preventDefault();
		$('body').removeClass('main-content-body-show');
	})
	$('body').append('<div class="main-navbar-backdrop"></div>');
	$('.main-navbar-backdrop').on('click touchstart', function() {
		$('body').removeClass('main-navbar-show');
	});


	// ______________Dropdown menu
	$(document).on('click touchstart', function(e) {
		e.stopPropagation();
		var dropTarg = $(e.target).closest('.main-header .dropdown').length;
		if (!dropTarg) {
			$('.main-header .dropdown').removeClass('show');
		}
		if (window.matchMedia('(min-width: 992px)').matches) {
			var navTarg = $(e.target).closest('.main-navbar .nav-item').length;
			if (!navTarg) {
				$('.main-navbar .show').removeClass('show');
			}
			var menuTarg = $(e.target).closest('.main-header-menu .nav-item').length;
			if (!menuTarg) {
				$('.main-header-menu .show').removeClass('show');
			}
			if ($(e.target).hasClass('main-menu-sub-mega')) {
				$('.main-header-menu .show').removeClass('show');
			}
		} else {
			if (!$(e.target).closest('#mainMenuShow').length) {
				var hm = $(e.target).closest('.main-header-menu').length;
				if (!hm) {
					$('body').removeClass('main-header-menu-show');
				}
			}
		}
	});

	// ______________MainMenuShow
	$('#mainMenuShow').on('click', function(e) {
		e.preventDefault();
		$('body').toggleClass('main-header-menu-show');
	})
	$('.main-header-menu .with-sub').on('click', function(e) {
		e.preventDefault();
		$(this).parent().toggleClass('show');
		$(this).parent().siblings().removeClass('show');
	})
	$('.main-header-menu-header .close').on('click', function(e) {
		e.preventDefault();
		$('body').removeClass('main-header-menu-show');
	})

	

	// ______________Popover
	var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
		var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
		return new bootstrap.Popover(popoverTriggerEl)
	})

	// ______________Tooltip
	var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
	var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
  	return new bootstrap.Tooltip(tooltipTriggerEl)
	})


	// ______________Toast
	$(".toast").toast();

	// ______________Back-top-button
	$(window).on("scroll", function(e) {
		if ($(this).scrollTop() > 0) {
			$('#back-to-top').fadeIn('slow');
		} else {
			$('#back-to-top').fadeOut('slow');
		}
	});
	$(document).on("click", "#back-to-top", function(e) {
		$("html, body").animate({
			scrollTop: 0
		}, 0);
		return false;
	});

	// ______________Full screen
	$(document).on("click", ".fullscreen-button", function toggleFullScreen() {
		$('html').addClass('fullscreen');
		if ((document.fullScreenElement !== undefined && document.fullScreenElement === null) || (document.msFullscreenElement !== undefined && document.msFullscreenElement === null) || (document.mozFullScreen !== undefined && !document.mozFullScreen) || (document.webkitIsFullScreen !== undefined && !document.webkitIsFullScreen)) {
			if (document.documentElement.requestFullScreen) {
				document.documentElement.requestFullScreen();
			} else if (document.documentElement.mozRequestFullScreen) {
				document.documentElement.mozRequestFullScreen();
			} else if (document.documentElement.webkitRequestFullScreen) {
				document.documentElement.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
			} else if (document.documentElement.msRequestFullscreen) {
				document.documentElement.msRequestFullscreen();
			}
		} else {
			$('html').removeClass('fullscreen');
			if (document.cancelFullScreen) {
				document.cancelFullScreen();
			} else if (document.mozCancelFullScreen) {
				document.mozCancelFullScreen();
			} else if (document.webkitCancelFullScreen) {
				document.webkitCancelFullScreen();
			} else if (document.msExitFullscreen) {
				document.msExitFullscreen();
			}
		}
	})

	// ______________Cover Image
	$(".cover-image").each(function() {
		var attr = $(this).attr('data-image-src');
		if (typeof attr !== typeof undefined && attr !== false) {
			$(this).css('background', 'url(' + attr + ') center center');
		}
	});


	// ______________Horizontal-menu Active Class
	function addActiveClass(element) {
		if (current === "") {
		  if (element.attr('href').indexOf("#") !== -1) {
			element.parents('.main-navbar .nav-item').last().removeClass('active');
			if (element.parents('.main-navbar .nav-sub').length) {
			  element.parents('.main-navbar .nav-sub-item').last().removeClass('active');
			}
		  }
		} else {
			if (element.attr('href').indexOf(current) !== -1) {
				element.parents('.main-navbar .nav-item').last().addClass('active');
				if (element.parents('.main-navbar .nav-sub').length) {
				   element.parents('.main-navbar .nav-sub-item').last().addClass('active');
				}
			}
		}
	};
	var current = location.pathname.split("/").slice(-1)[0].replace(/^\/|\/$/g, '');
	$('.main-navbar .nav li a').each(function() {
	  var $this = $(this);
	  addActiveClass($this);
	});
	



	// ______________ SWITCHER-toggle ______________//
	
		/************Theme Layouts************/

	//$('body').addClass('theme-style');
	//$('body').addClass('light-theme');
	//$('body').addClass('dark-theme');

	/*Header Styles*/
	//$('body').addClass('color-header');
	//$('body').addClass('header-dark');
	
	/*Horizontal-menu Styles*/
	//$('body').addClass('light-horizontal');
	// $('body').addClass('color-horizontal');
	
	/*Left-menu Styles*/
	//$('body').addClass('light-leftmenu');
	//$('body').addClass('color-leftmenu');
	
	/*Leftmenu Icon-Style*/
	//$('body').addClass('icon-style');
	
	
	/*rtl version */
	//  $('body').addClass('rtl');

	/***********side-menu styles*******************/

	/*closed-sidemenu version */
	//  $('body').addClass('closed');
	//  $('body').addClass('main-sidebar-hide');

	/*hover-submenu1 version */
	//  $('body').addClass('hover-submenu');
	//  $('body').addClass('main-sidebar-hide');

	/*hover-submenu1 version */
	//  $('body').addClass('hover-submenu1');
	//  $('body').addClass('main-sidebar-hide');

	/*icon-overlay version */
	//  $('body').addClass('icon-overlay');
	//  $('body').addClass('main-sidebar-hide');

	/*icon-text version */
	//  $('body').addClass('icon-text');
	//   $('body').addClass('main-sidebar-hide');


});
	
$(document).ready (function(){
	let bodyiconText = $('body').hasClass('icon-text');
	if (bodyiconText) {
			$('body').addClass('icon-text');
            localStorage.setItem("icon-text", "True");
            $("head link#style").attr("href", $(this));
            (document.getElementById("style")?.setAttribute("href", "../../assets/plugins/sidemenu/sidemenu2.js"));
        } 
		else {
			$('body').removeClass('icon-text');
            localStorage.setItem("icon-text", "false");
            $("head link#style").attr("href", $(this));
            (document.getElementById("style")?.setAttribute("href", "../../assets/plugins/sidemenu/sidemenu.js"));
        };
});
		
$(document).ready (function(){
	let bodyRtl = $('body').hasClass('rtl');
	if (bodyRtl) {
			$('body').addClass('rtl');
            localStorage.setItem("rtl", "True");
            $("head link#style").attr("href", $(this));
            (document.getElementById("style")?.setAttribute("href", "../../assets/plugins/bootstrap/css/bootstrap.rtl.min.css"));
        } 
		else {
			$('body').removeClass('rtl');
            localStorage.setItem("rtl", "false");
            $("head link#style").attr("href", $(this));
            (document.getElementById("style")?.setAttribute("href", "../../assets/plugins/bootstrap/css/bootstrap.min.css"));
        };
});



