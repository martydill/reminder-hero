$(document).ready(function() {
	
	$('a[href*=#]').click(function() {
	    if (location.pathname.replace(/^\//,'') == this.pathname.replace(/^\//,'')
	    && location.hostname == this.hostname) {
	      var $target = $(this.hash);
	      $target = $target.length && $target
	      || $('[name=' + this.hash.slice(1) +']');
	      if ($target.length) {
	        var targetOffset = $target.offset().top;
	        $('html,body')
	        .animate({scrollTop: targetOffset}, {duration: 1000, easing: 'easeInOutExpo'});
	       return false;
	      }
	    }
	  });
	
	// Values in Forms
	$('#subForm .email').watermark();
	$('#name-field').watermark();
	$('#email-field').watermark();
	$('#phone-field').watermark();
	$('#company-field').watermark();
	$('#comments-field').watermark();
	$('#author').watermark();
	$('#email').watermark();
	$('#url').watermark();
	$('#comment').watermark();

	
	//Form Validation
	$(".theme-form").each(function() {
		$(this).validate();
	});

    $(".scroll-top").click(function() {
        $('.sign-up-form input[type="email"]').stop().css("background-color", "#cbeefa");
    });
});