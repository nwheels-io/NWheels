'use strict';

$(document).ready(function () {

    var uiStates = {
        New: { canEdit: true, canApply: false, hasOutput: false },
        Modified: { canEdit: true, canApply: true, hasOutput: false },
        NotValid: { canEdit: true, canApply: false, hasOutput: false },
        Saving: { canEdit: false, canApply: false, hasOutput: false },
        Saved: { canEdit: true, canApply: false, hasOutput: true },
        SaveFailed: { canEdit: true, canApply: true, hasOutput: false },
    };

    setUIState('New');

    $('input[type=text]').keyup(function () {
        if (validateForm()) {
            setUIState('Modified');
        }
    });

    $('#Transaction_SubmitCommand__Button').click(function () {
        if (!validateForm()) {
            return false;
        }

        setUIState('Saving');

        var data = {
            name: $('#Transaction_ViewModel_Name__Input').val(),
        };

        $.ajax({
            type: 'POST',
            url: '/tx/HelloWorld/Hello',
            data: JSON.stringify(data),
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                $('#Transaction_ViewModel_Message__Text').text(data.result)
            }
        })
        .done(function (data) {
            setUIState('Saved');
        })
        .fail(function () {
            setUIState('SaveFailed');
        })
    });

    function validateForm() {
        var form = $('form');
        form.validate();
        if (!form.valid()) {
            setUIState('NotValid');
            return false;
        }
        return true;
    }

    function setUIState(name) {
        var state = uiStates[name];

        $('#Transaction_SubmitCommand__Button').prop('disabled', !state.canApply);
        $('input[type=text]').prop('disabled', !state.canEdit);
        $('#Transaction_ViewModel_Message__Label').toggle(state.hasOutput);
        $('#Transaction_ViewModel_Message__Text').toggle(state.hasOutput);
    }
});
