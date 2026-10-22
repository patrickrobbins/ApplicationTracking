/* DependencyTracker global site helpers */
(function () {
    'use strict';

    $(function () {
        // Auto-dismiss alerts after 5 seconds
        window.setTimeout(function () {
            $('.alert.alert-success, .alert.alert-info').fadeOut(400);
        }, 5000);

        // Auto-hide toasts
        $(document).on('hidden.bs.toast', '.toast', function () {
            $(this).remove();
        });
    });

    // Send the anti-forgery token with every jQuery AJAX request. Endpoints
    // that cannot receive a form token (for example the remote validation
    // used by jQuery Validate) read it back from the X-CSRF-Token header.
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            if (!settings || settings.type === 'GET') {
                return;
            }
            var token = $('input[name="__RequestVerificationToken"]').first().val();
            if (token && !settings.headers['X-CSRF-Token']) {
                settings.headers['X-CSRF-Token'] = token;
            }
        }
    });
})();
