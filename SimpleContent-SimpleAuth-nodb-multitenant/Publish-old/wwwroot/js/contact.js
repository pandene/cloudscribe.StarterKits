(function () {

    function objectToUrl(obj) {
        var string = '';

        for (var prop in obj) {
            if (obj.hasOwnProperty(prop)) {
                string += prop + '=' + obj[prop].replace(/ /g, '+') + '&';
            }
        }

        return string.substring(0, string.length - 1);
    }

    var AsynObject = AsynObject ? AsynObject : {};

    AsynObject.ajax = function (url, callback) {
        var ajaxRequest = AsynObject.getAjaxRequest(callback);
        ajaxRequest.open("GET", url, true);
        ajaxRequest.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        ajaxRequest.send(null);
    };

    AsynObject.postAjax = function (url, callback, data) {
        var ajaxRequest = AsynObject.getAjaxRequest(callback);
        ajaxRequest.open("POST", url, true);
        ajaxRequest.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        ajaxRequest.send(objectToUrl(data));
    };

    AsynObject.getAjaxRequest = function (callback) {

        var ajaxRequest = new XMLHttpRequest();

        ajaxRequest.onreadystatechange = function () {
            if (ajaxRequest.readyState > 1 && ajaxRequest.status > 0) {
                callback(ajaxRequest.readyState, ajaxRequest.status, ajaxRequest.responseText);
            }
        };

        return ajaxRequest;
    };

    function hasClass(elem, className) {
        return new RegExp(' ' + className + ' ').test(' ' + elem.className + ' ');
    }

    function addClass(elem, className) {
        if (!hasClass(elem, className)) {
            elem.className += ' ' + className;
        }
    }

    function removeClass(elem, className) {
        var newClass = ' ' + elem.className.replace(/[\t\r\n]/g, ' ') + ' ';
        if (hasClass(elem, className)) {
            while (newClass.indexOf(' ' + className + ' ') >= 0) {
                newClass = newClass.replace(' ' + className + ' ', ' ');
            }
            elem.className = newClass.replace(/^\s+|\s+$/g, '');
        }
    }



    function saveContact(name, email, website, content, captchaResult, callback) {

        if (localStorage) {
            localStorage.setItem("name", name);
            localStorage.setItem("email", email);
            localStorage.setItem("website", website);
        }
        endpoint = document.getElementById("contactform").getAttribute("data-contact-path");
        AsynObject.postAjax(endpoint, function (state, status, data) {

            var elemStatus = document.getElementById("status");
            if (state === 4 && status === 200) {
                elemStatus.innerHTML = "Your request has been submitted";
                removeClass(elemStatus, "alert-danger");
                addClass(elemStatus, "alert-success");

                document.getElementById("contactcontent").value = "";

                callback(true);

                return;
            } else if (status !== 200) {
                addClass(elemStatus, "alert-danger");
                elemStatus.innerText = "Unable to submit contact request";
                callback(false);
            }
        }, {
                mode: "save",
                //postId: postId,
                name: name,
                email: email,
                website: website,
                content: content,
                "g-recaptcha-response": captchaResult,
                __RequestVerificationToken: document.querySelector("input[name=__RequestVerificationToken]").getAttribute("value")
            });

    }


    function initialize() {
        //postId = document.querySelector("[itemprop=blogPost]").getAttribute("data-id");
        endpoint = document.getElementById("contactform").getAttribute("data-contact-path");
        var email = document.getElementById("contactemail");
        var name = document.getElementById("contactname");
        var website = document.getElementById("contacturl");
        var content = document.getElementById("contactcontent");
        var contactForm = document.getElementById("contactform");
        var recaptcha;






        contactForm.onsubmit = function (e) {
            e.preventDefault();
            var button = e.target;
            button.setAttribute("disabled", true);
            var recpatchaValue = "";
            recaptcha = document.getElementById("g-recaptcha-response");
            if (recaptcha) {
                recpatchaValue = recaptcha.value;
                if (recpatchaValue.length == 0) { return; }
            }

            saveContact(name.value, email.value, website.value, content.value, recpatchaValue, function () {
                button.removeAttribute("disabled");
                if (recaptcha) {
                    grecaptcha.reset();
                }

            });
        };

        website.addEventListener("keyup", function (e) {
            var w = e.target;
            if (w.value.trim().length >= 4 && w.value.indexOf("http") === -1) {
                w.value = "http://" + w.value;
            }
        });

        window.addEventListener("click", function (e) {
            var tag = e.target;

            if (hasClass(tag, "deletecomment")) {
                var comment = getParentsByAttribute(tag, "itemprop", "comment")[0];
                deleteComment(comment.getAttribute("data-id"), comment);
            }
            if (hasClass(tag, "approvecomment")) {
                var comment = getParentsByAttribute(tag, "itemprop", "comment")[0];
                approveComment(comment.getAttribute("data-id"), tag);
            }
        });

        if (localStorage) {
            email.value = localStorage.getItem("email");
            website.value = localStorage.getItem("website");

            if (name.value.length === 0) {
                name.value = localStorage.getItem("name");
            }
        }
    }

    if (document.getElementById("contactform")) {
        initialize();
    }
})();