document.addEventListener('DOMContentLoaded', function () {
 
    const customModal = document.getElementById('customConfirmModal');
    const modalMessage = document.getElementById('modalConfirmMessage');
    const confirmButton = document.getElementById('modalConfirmBtn');
    const cancelButton = document.getElementById('modalCancelBtn');
    const closeSpan = document.querySelector('.close-modal-btn');

    let currentForm = null; 

  
    window.showCustomConfirm = function (message, form) {
        modalMessage.textContent = message;
        currentForm = form;
        customModal.style.display = 'flex'; 
    };

    function hideCustomConfirm() {
        customModal.style.display = 'none';
        currentForm = null;
    }

   
    confirmButton.addEventListener('click', function () {
        if (currentForm) {
            currentForm.submit();
        }
        hideCustomConfirm();
    });

    cancelButton.addEventListener('click', function () {
        hideCustomConfirm();
    });

  
    closeSpan.addEventListener('click', function () {
        hideCustomConfirm();
    });

    window.addEventListener('click', function (event) {
        if (event.target == customModal) {
            hideCustomConfirm();
        }
    });


    document.querySelectorAll('.action-btn').forEach(button => {
      
        const originalOnClick = button.getAttribute('onclick');
        if (originalOnClick) {
            const match = originalOnClick.match(/confirm\('(.*?)'\)/);
            if (match && match[1]) {
                const message = match[1];
                button.removeAttribute('onclick'); 
                button.addEventListener('click', function (event) {
                    event.preventDefault(); 
                    showCustomConfirm(message, button.closest('form'));
                });
            }
        }
    });
});
  
 