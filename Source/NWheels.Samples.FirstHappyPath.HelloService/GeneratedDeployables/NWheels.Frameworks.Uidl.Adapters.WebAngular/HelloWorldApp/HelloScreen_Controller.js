'use strict';

$(document).ready(function () {

    var uiStates = {
        New: { canEdit: true, canApply: false, text: 'New transaction', style: 'primary' },
        Modified: { canEdit: true, canApply: true, text: 'Input modified. Click GO to apply changes.', style: 'warning' },
        NotValid: { canEdit: true, canApply: false, text: 'Invalid entry', style: 'error' },
        Saving: { canEdit: false, canApply: false, text: 'Applying changes...', style: 'progress' },
        Saved: { canEdit: true, canApply: false, text: 'Changes successfully applied.', style: 'success' },
        SaveFailed: { canEdit: true, canApply: true, text: 'Failed to apply changes', style: 'error' }
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
            name: $('#Transaction_ViewModel_Name').val(),
        };

        $.ajax({
            type: 'POST',
            url: '/tx/HelloWorldTx/Hello',
            data: JSON.stringify(data),
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                $('#Transaction__Status').text(data.result)
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
        $('#Transaction__Status').text(state.text).prop('class', state.style);
        $('input[type=text]').prop('disabled', !state.canEdit);
    }
});
