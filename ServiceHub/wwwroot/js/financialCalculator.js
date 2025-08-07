document.addEventListener('DOMContentLoaded', function () {
    const revenueItemsContainer = document.getElementById('revenueItemsContainer');
    const addRevenueItemBtn = document.getElementById('addRevenueItemBtn');
    const expenseItemsContainer = document.getElementById('expenseItemsContainer');
    const addExpenseItemBtn = document.getElementById('addExpenseItemBtn');

    let revenueItemCounter = 0;
    let expenseItemCounter = 0;


    function addRevenueItemRow(description = '', amount = '') {
        const itemRow = document.createElement('div');
        itemRow.classList.add('item-row');
        itemRow.dataset.index = revenueItemCounter;

        itemRow.innerHTML = `
                    <div class="form-group flex-grow-1">
                        <label for="revenueDescription_${revenueItemCounter}">Описание:</label>
                        <input type="text" class="form-control" id="revenueDescription_${revenueItemCounter}" name="Revenues[${revenueItemCounter}].Description" value="${description}" required maxlength="200" placeholder="Напр. Продажби на продукти">
                        <div class="text-danger" id="revenueDescription_${revenueItemCounter}Error"></div>
                    </div>
                    <div class="form-group" style="width: 150px;">
                        <label for="revenueAmount_${revenueItemCounter}">Сума (лв.):</label>
                        <input type="number" step="0.01" class="form-control" id="revenueAmount_${revenueItemCounter}" name="Revenues[${revenueItemCounter}].Amount" value="${amount}" required min="0.01" max="1000000000">
                        <div class="text-danger" id="revenueAmount_${revenueItemCounter}Error"></div>
                    </div>
                    <button type="button" class="remove-item-btn" data-type="revenue" data-index="${revenueItemCounter}">
                        <i class="fas fa-trash"></i>
                    </button>
                `;
        revenueItemsContainer.appendChild(itemRow);
        itemRow.querySelector('.remove-item-btn').addEventListener('click', removeRevenueItem);
        revenueItemCounter++;
    }

    function removeRevenueItem(event) {
        event.target.closest('.item-row').remove();
        reindexItems('revenue');
    }


    function addExpenseItemRow(description = '', amount = '') {
        const itemRow = document.createElement('div');
        itemRow.classList.add('item-row');
        itemRow.dataset.index = expenseItemCounter;

        itemRow.innerHTML = `
                    <div class="form-group flex-grow-1">
                        <label for="expenseDescription_${expenseItemCounter}">Описание:</label>
                        <input type="text" class="form-control" id="expenseDescription_${expenseItemCounter}" name="Expenses[${expenseItemCounter}].Description" value="${description}" required maxlength="200" placeholder="Напр. Наем на офис">
                        <div class="text-danger" id="expenseDescription_${expenseItemCounter}Error"></div>
                    </div>
                    <div class="form-group" style="width: 150px;">
                        <label for="expenseAmount_${expenseItemCounter}">Сума (лв.):</label>
                        <input type="number" step="0.01" class="form-control" id="expenseAmount_${expenseItemCounter}" name="Expenses[${expenseItemCounter}].Amount" value="${amount}" required min="0.01" max="1000000000">
                        <div class="text-danger" id="expenseAmount_${expenseItemCounter}Error"></div>
                    </div>
                    <button type="button" class="remove-item-btn" data-type="expense" data-index="${expenseItemCounter}">
                        <i class="fas fa-trash"></i>
                    </button>
                `;
        expenseItemsContainer.appendChild(itemRow);
        itemRow.querySelector('.remove-item-btn').addEventListener('click', removeExpenseItem);
        expenseItemCounter++;
    }

    function removeExpenseItem(event) {
        event.target.closest('.item-row').remove();
        reindexItems('expense');
    }


    function reindexItems(type) {
        const container = type === 'revenue' ? revenueItemsContainer : expenseItemsContainer;
        const rows = container.querySelectorAll('.item-row');
        rows.forEach((row, index) => {
            row.dataset.index = index;
            const prefix = type === 'revenue' ? 'Revenue' : 'Expense';

            row.querySelector(`[id^="${prefix.toLowerCase()}Description_"]`).id = `${prefix.toLowerCase()}Description_${index}`;
            row.querySelector(`[name^="${prefix}s["]`).name = `${prefix}s[${index}].Description`;
            row.querySelector(`[id^="${prefix.toLowerCase()}Description_"]`).nextElementSibling.id = `${prefix.toLowerCase()}Description_${index}Error`;

            row.querySelector(`[id^="${prefix.toLowerCase()}Amount_"]`).id = `${prefix.toLowerCase()}Amount_${index}`;
            row.querySelector(`[name$=".Amount"]`).name = `${prefix}s[${index}].Amount`;
            row.querySelector(`[id^="${prefix.toLowerCase()}Amount_"]`).nextElementSibling.id = `${prefix.toLowerCase()}Amount_${index}Error`;

            row.querySelector('.remove-item-btn').dataset.index = index;
        });
        if (type === 'revenue') {
            revenueItemCounter = rows.length;
        } else {
            expenseItemCounter = rows.length;
        }
    }


    addRevenueItemRow();
    addExpenseItemRow();


    addRevenueItemBtn.addEventListener('click', () => addRevenueItemRow());
    addExpenseItemBtn.addEventListener('click', () => addExpenseItemRow());


    document.getElementById('financialCalculatorForm').addEventListener('submit', async function (event) {
        event.preventDefault();

        const form = event.target;
        const messageArea = document.getElementById('messageArea');


        messageArea.innerHTML = '';
        document.querySelectorAll('.text-danger').forEach(el => el.textContent = '');

        messageArea.innerHTML = '<div class="alert alert-info"><i class="fas fa-spinner fa-spin me-2"></i>Изчисляване на финансов отчет... Моля, изчакайте.</div>';

        const requestData = {
            netProfitForROI: parseFloat(document.getElementById('netProfitForROI').value) || 0,
            costOfInvestment: parseFloat(document.getElementById('costOfInvestment').value) || 0,
            growthRatePercentage: parseFloat(document.getElementById('growthRatePercentage').value) || 0,
            forecastPeriodMonths: parseInt(document.getElementById('forecastPeriodMonths').value) || 0,
            notes: document.getElementById('notes').value,
            revenues: [],
            expenses: []
        };

        let isValid = true;


        if (isNaN(requestData.netProfitForROI)) {
            document.getElementById('netProfitForROIError').textContent = 'Моля, въведете валидна сума.';
            isValid = false;
        }
        if (isNaN(requestData.costOfInvestment) || requestData.costOfInvestment <= 0) {
            document.getElementById('costOfInvestmentError').textContent = 'Моля, въведете положителна сума за цена на инвестицията.';
            isValid = false;
        }


        const revenueRows = revenueItemsContainer.querySelectorAll('.item-row');
        revenueRows.forEach((row, index) => {
            const descriptionInput = row.querySelector(`[name="Revenues[${index}].Description"]`);
            const amountInput = row.querySelector(`[name="Revenues[${index}].Amount"]`);

            const item = {
                description: descriptionInput.value,
                amount: parseFloat(amountInput.value)
            };

            if (!item.description.trim()) {
                document.getElementById(`revenueDescription_${index}Error`).textContent = 'Описанието е задължително.';
                isValid = false;
            } else if (item.description.length > 200) {
                document.getElementById(`revenueDescription_${index}Error`).textContent = 'Описанието не може да надвишава 200 символа.';
                isValid = false;
            }
            if (isNaN(item.amount) || item.amount <= 0 || item.amount > 1000000000) {
                document.getElementById(`revenueAmount_${index}Error`).textContent = 'Сумата трябва да е положително число до 1 000 000 000.';
                isValid = false;
            }
            requestData.revenues.push(item);
        });


        const expenseRows = expenseItemsContainer.querySelectorAll('.item-row');
        expenseRows.forEach((row, index) => {
            const descriptionInput = row.querySelector(`[name="Expenses[${index}].Description"]`);
            const amountInput = row.querySelector(`[name="Expenses[${index}].Amount"]`);

            const item = {
                description: descriptionInput.value,
                amount: parseFloat(amountInput.value)
            };

            if (!item.description.trim()) {
                document.getElementById(`expenseDescription_${index}Error`).textContent = 'Описанието е задължително.';
                isValid = false;
            } else if (item.description.length > 200) {
                document.getElementById(`expenseDescription_${index}Error`).textContent = 'Описанието не може да надвишава 200 символа.';
                isValid = false;
            }
            if (isNaN(item.amount) || item.amount <= 0 || item.amount > 1000000000) {
                document.getElementById(`expenseAmount_${index}Error`).textContent = 'Сумата трябва да е положително число до 1 000 000 000.';
                isValid = false;
            }
            requestData.expenses.push(item);
        });


        if (isNaN(requestData.growthRatePercentage) || requestData.growthRatePercentage < 0 || requestData.growthRatePercentage > 100) {
            document.getElementById('growthRatePercentageError').textContent = 'Процентът на растеж трябва да е между 0 и 100.';
            isValid = false;
        }
        if (isNaN(requestData.forecastPeriodMonths) || requestData.forecastPeriodMonths < 1 || requestData.forecastPeriodMonths > 120) {
            document.getElementById('forecastPeriodMonthsError').textContent = 'Периодът на прогноза трябва да е между 1 и 120 месеца.';
            isValid = false;
        }
        if (requestData.notes.length > 1000) {
            document.getElementById('notesError').textContent = 'Бележките не могат да надвишават 1000 символа.';
            isValid = false;
        }

        if (!isValid) {
            messageArea.innerHTML = '<div class="alert alert-danger">Моля, коригирайте грешките във формата.</div>';
            return;
        }

        try {
            const response = await fetch('/api/FinancialCalculator/calculate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(requestData)
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;

                const contentDisposition = response.headers.get('Content-Disposition');
                let filename = "financial_report.pdf";
                if (contentDisposition && contentDisposition.indexOf('filename=') !== -1) {
                    const filenameMatch = /filename\*?=['"]?(?:UTF-8'')?([^;"]+)/i.exec(contentDisposition);
                    if (filenameMatch && filenameMatch[1]) {
                        filename = decodeURIComponent(filenameMatch[1].replace(/\+/g, ' '));
                    }
                }

                a.download = filename;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                a.remove();

                messageArea.innerHTML = `<div class="alert alert-success"><i class="fas fa-check-circle me-2"></i>Файлът на финансовия отчет '${filename}' е успешно генериран и изтеглен!</div>`;
            } else {
                let errorText = await response.text();
                try {
                    const errorJson = JSON.parse(errorText);
                    if (errorJson.errors) {

                        for (const key in errorJson.errors) {
                            if (errorJson.errors.hasOwnProperty(key)) {
                                const errorMessages = errorJson.errors[key].join('; ');

                                const mainFieldId = key.charAt(0).toLowerCase() + key.slice(1) + 'Error';
                                if (document.getElementById(mainFieldId)) {
                                    document.getElementById(mainFieldId).textContent = errorMessages;
                                }

                                const itemMatch = key.match(/(Revenues|Expenses)\[(\d+)\]\.(Description|Amount)/);
                                if (itemMatch) {
                                    const itemType = itemMatch[1].toLowerCase();
                                    const itemIndex = itemMatch[2];
                                    const itemField = itemMatch[3].toLowerCase();
                                    const dynamicErrorId = `${itemType.slice(0, -1)}${itemField}_${itemIndex}Error`;
                                    if (document.getElementById(dynamicErrorId)) {
                                        document.getElementById(dynamicErrorId).textContent = errorMessages;
                                    }
                                }
                            }
                        }
                    }
                    errorText = errorJson.message || response.statusText;
                } catch (e) {
                    console.error("Failed to parse error response as JSON:", e);
                }
                messageArea.innerHTML = `<div class="alert alert-danger"><i class="fas fa-times-circle me-2"></i>Грешка при генериране на финансов отчет: ${errorText}</div>`;
            }
        } catch (error) {
            console.error('Fetch error:', error);
            messageArea.innerHTML = `<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Възникна мрежова грешка: ${error.message}.</div>`;
        }
    });
});