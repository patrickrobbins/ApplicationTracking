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
})();
