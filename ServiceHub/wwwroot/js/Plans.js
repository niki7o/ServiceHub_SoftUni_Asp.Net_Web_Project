document.addEventListener('DOMContentLoaded', function () {
    const openPaymentModalButton = document.getElementById('openPaymentModalButton');
    const paymentModal = document.getElementById('paymentModal');
    const closeButton = document.querySelector('.close-button');
    const paymentForm = document.getElementById('paymentForm');
    const payButton = document.getElementById('payButton');
    const loadingSpinner = document.getElementById('loadingSpinner');
    const paymentMessage = document.getElementById('paymentMessage');

    const cardNumberInput = document.getElementById('cardNumber');
    const cardNameInput = document.getElementById('cardName');
    const expiryDateInput = document.getElementById('expiryDate');
    const cvcInput = document.getElementById('cvc');

    const cardNumberError = document.getElementById('cardNumberError');
    const cardNameError = document.getElementById('cardNameError');
    const expiryDateError = document.getElementById('expiryDateError');
    const cvcError = document.getElementById('cvcError');


    if (openPaymentModalButton) {
        openPaymentModalButton.addEventListener('click', function () {
            paymentModal.style.display = 'flex';

            paymentForm.reset();
            paymentMessage.style.display = 'none';
            paymentMessage.className = 'alert-message';
            hideAllValidationErrors();
            payButton.disabled = false;
            loadingSpinner.style.display = 'none';
        });
    }


    closeButton.addEventListener('click', function () {
        paymentModal.style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        if (event.target == paymentModal) {
            paymentModal.style.display = 'none';
        }
    });


    cardNumberInput.addEventListener('input', function (e) {
        let value = e.target.value.replace(/\s+/g, '');
        let formattedValue = '';
        for (let i = 0; i < value.length; i++) {
            if (i > 0 && i % 4 === 0) {
                formattedValue += ' ';
            }
            formattedValue += value[i];
        }
        e.target.value = formattedValue;
    });


    expiryDateInput.addEventListener('input', function (e) {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length > 2) {
            value = value.substring(0, 2) + '/' + value.substring(2, 4);
        }
        e.target.value = value;
    });

    function showValidationError(element, message) {
        element.textContent = message;
        element.style.display = 'block';
    }

    function hideValidationError(element) {
        element.textContent = '';
        element.style.display = 'none';
    }

    function hideAllValidationErrors() {
        hideValidationError(cardNumberError);
        hideValidationError(cardNameError);
        hideValidationError(expiryDateError);
        hideValidationError(cvcError);
    }

    function validateForm() {
        let isValid = true;
        hideAllValidationErrors();


        const cardNumber = cardNumberInput.value.replace(/\s/g, '');
        if (!/^(5[1-5]\d{14})$/.test(cardNumber) || cardNumber.length !== 16) {
            showValidationError(cardNumberError, 'Невалиден номер на Mastercard (16 цифри, започва с 51-55).');
            isValid = false;
        }


        if (cardNameInput.value.trim() === '') {
            showValidationError(cardNameError, 'Името на картодържателя е задължително.');
            isValid = false;
        }


        const expiryDate = expiryDateInput.value;
        const expiryRegex = /^(0[1-9]|1[0-2])\/?([0-9]{2})$/;
        if (!expiryRegex.test(expiryDate)) {
            showValidationError(expiryDateError, 'Невалиден формат (ММ/ГГ).');
            isValid = false;
        } else {
            const [month, year] = expiryDate.split('/').map(Number);
            const currentYear = new Date().getFullYear() % 100;
            const currentMonth = new Date().getMonth() + 1;

            if (year < currentYear || (year === currentYear && month < currentMonth)) {
                showValidationError(expiryDateError, 'Картата е изтекла.');
                isValid = false;
            }
        }


        const cvc = cvcInput.value;
        if (!/^\d{3,4}$/.test(cvc)) {
            showValidationError(cvcError, 'CVC трябва да е 3 или 4 цифри.');
            isValid = false;
        }

        return isValid;
    }

    paymentForm.addEventListener('submit', function (e) {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        payButton.disabled = true;
        loadingSpinner.style.display = 'block';
        paymentMessage.style.display = 'none';


        setTimeout(function () {

            const isPaymentSuccessful = true;

            if (isPaymentSuccessful) {

                fetch('/api/Subscription/subscribe', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ confirmSubscription: true })
                })
                    .then(response => {
                        if (!response.ok) {
                            return response.json().then(err => { throw new Error(err.message || 'Грешка при надграждане.'); });
                        }
                        return response.json();
                    })
                    .then(data => {
                        loadingSpinner.style.display = 'none';
                        paymentMessage.style.display = 'block';
                        if (data.success) {
                            paymentMessage.textContent = data.message;
                            paymentMessage.className = 'alert-message alert-success-custom';

                            if (openPaymentModalButton) {
                                openPaymentModalButton.disabled = true;
                                openPaymentModalButton.textContent = 'Вече сте Бизнес Потребител';
                                openPaymentModalButton.className = 'btn-current';
                            }

                            setTimeout(() => {
                                paymentModal.style.display = 'none';
                                window.location.reload();
                            }, 2000);
                        } else {
                            paymentMessage.textContent = data.message;
                            paymentMessage.className = 'alert-message alert-danger-custom';
                            payButton.disabled = false;
                        }
                    })
                    .catch(error => {
                        loadingSpinner.style.display = 'none';
                        paymentMessage.style.display = 'block';
                        paymentMessage.textContent = `Възникна грешка: ${error.message}`;
                        paymentMessage.className = 'alert-message alert-danger-custom';
                        payButton.disabled = false;
                        console.error('Error upgrading user:', error);
                    });

            } else {
                loadingSpinner.style.display = 'none';
                paymentMessage.style.display = 'block';
                paymentMessage.textContent = 'Плащането беше неуспешно. Моля, опитайте отново.';
                paymentMessage.className = 'alert-message alert-danger-custom';
                payButton.disabled = false;
            }
        }, 2000);
    });
});