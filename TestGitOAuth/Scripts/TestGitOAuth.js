(function(global, $, undefined) {

    $.ajax({
        url: "/home/getaccesstoken",
        success: function() {
            var accessToken = arguments[0];
            if (accessToken) {
                $(".token").text(accessToken).parent().show();
                listRepos();
            } else {
                $(".requestToken").show();
            }
        },
        error: function(jqXHR, statusText, errorText) {
            alert(errorText);
        },
        dataType: "json"
    });

    function listRepos() {
        $.ajax({
            url: "/websites/getrepos",
            success: function() {
                var repos = arguments[0];
                var $select = $(".two select");
                $select.find('option').remove();
                for (var index in repos) {
                    var repo = repos[index];
                    $select.append($("<option></option>").attr("value", repo.url).text(repo.full_name));
                }
                $(".two").show();

                $(".three .buttonHook").click(function() {
                    listHooks($select.val());
                });
                $(".three").show();
            },
            error: function(jqXHR, statusText, errorText) {
                alert(errorText);
            },
            dataType: "json"
        });
    }

    function listHooks(url) {
        var $hooks = $(".three .hooks");
        $.ajax({
            url: "/websites/gethooks",
            data: { url: url },
            success: function() {
                var hooks = arguments[0];
                $hooks.val(hooks);
            },
            error: function(jqXHR, statusText, errorText) {
                alert(errorText);
            },
            dataType: "json"
        });
    }


})(this, jQuery);