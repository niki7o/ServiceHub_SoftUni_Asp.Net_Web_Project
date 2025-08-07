document.addEventListener('DOMContentLoaded', function() {
    const invoiceItemsContainer = document.getElementById('invoiceItemsContainer');
    const addItemBtn = document.getElementById('addItemBtn');
    let itemCounter = 0;


    function addItemRow(description = '', quantity = '', unitPrice = '')
    {
        const itemRow = document.createElement('div');
        itemRow.classList.add('item-row');
        itemRow.dataset.index = itemCounter;

        itemRow.innerHTML = `
                    < div class= "form-group flex-grow-1" >
                        < label for= "itemDescription_${itemCounter}" > Описание:</ label >
                        < input type = "text" class= "form-control" id = "itemDescription_${itemCounter}" name = "Items[${itemCounter}].Description" value = "${description}" required maxlength = "200" placeholder="Напр. Услуга X">
                        <div class= "text-danger" id = "itemDescription_${itemCounter}Error" ></ div >
                    </ div >
                    < div class= "form-group" style = "width: 150px;" >
                        < label for= "itemQuantity_${itemCounter}" > Количество:</ label >
                        < input type = "number" step = "0.01" class= "form-control" id = "itemQuantity_${itemCounter}" name = "Items[${itemCounter}].Quantity" value = "${quantity}" required min = "0.01" max="1000000">
                        <div class= "text-danger" id = "itemQuantity_${itemCounter}Error" ></ div >
                    </ div >
                    < div class= "form-group" style = "width: 150px;" >
                        < label for= "itemUnitPrice_${itemCounter}" > Ед.цена(лв.):</ label >
                        < input type = "number" step = "0.01" class= "form-control" id = "itemUnitPrice_${itemCounter}" name = "Items[${itemCounter}].UnitPrice" value = "${unitPrice}" required min = "0.01" max="10000000">
                        <div class= "text-danger" id = "itemUnitPrice_${itemCounter}Error" ></ div >
                    </ div >
                    < button type = "button" class= "remove-item-btn" data - index = "${itemCounter}" >
                        < i class= "fas fa-trash" ></ i >
                    </ button >
                `;

invoiceItemsContainer.appendChild(itemRow);


itemRow.querySelector('.remove-item-btn').addEventListener('click', function() {
    itemRow.remove();
    reindexItems(); 
});

itemCounter++;
            }

            function reindexItems()
{
    const rows = invoiceItemsContainer.querySelectorAll('.item-row');
    rows.forEach((row, index) => {
    row.dataset.index = index;
    row.querySelector(`[id ^= "itemDescription_"]`).id = `itemDescription_${ index}`;
    row.querySelector(`[name ^= "Items["]`).name = `Items[${ index}].Description`;
    row.querySelector(`[id ^= "itemDescription_"]`).nextElementSibling.id = `itemDescription_${ index}
    Error`;

    row.querySelector(`[id ^= "itemQuantity_"]`).id = `itemQuantity_${ index}`;
    row.querySelector(`[name$= ".Quantity"]`).name = `Items[${ index}].Quantity`;
    row.querySelector(`[id ^= "itemQuantity_"]`).nextElementSibling.id = `itemQuantity_${ index}
    Error`;

    row.querySelector(`[id ^= "itemUnitPrice_"]`).id = `itemUnitPrice_${ index}`;
    row.querySelector(`[name$= ".UnitPrice"]`).name = `Items[${ index}].UnitPrice`;
    row.querySelector(`[id ^= "itemUnitPrice_"]`).nextElementSibling.id = `itemUnitPrice_${ index}
    Error`;

    row.querySelector('.remove-item-btn').dataset.index = index;
});
itemCounter = rows.length;
            }


          
            addItemRow();


addItemBtn.addEventListener('click', () => addItemRow());


document.getElementById('invoiceGeneratorForm').addEventListener('submit', async function(event) {
                event.preventDefault();

    const form = event.target;
    const messageArea = document.getElementById('messageArea');

    messageArea.innerHTML = '';
    document.querySelectorAll('.text-danger').forEach(el => el.textContent = '');

    messageArea.innerHTML = '<div class="alert alert-info"><i class="fas fa-spinner fa-spin me-2"></i>Генерирането на фактура е в ход... Моля, изчакайте.</div>';

    const requestData = {
                    invoiceNumber: document.getElementById('invoiceNumber').value,
                    issueDate: document.getElementById('issueDate').value,
                    sellerName: document.getElementById('sellerName').value,
                    sellerAddress: document.getElementById('sellerAddress').value,
                    sellerEIK: document.getElementById('sellerEIK').value,
                    sellerMOL: document.getElementById('sellerMOL').value,
                    buyerName: document.getElementById('buyerName').value,
                    buyerAddress: document.getElementById('buyerAddress').value,
                    buyerEIK: document.getElementById('buyerEIK').value,
                    buyerMOL: document.getElementById('buyerMOL').value,
                    discountPercentage: parseFloat(document.getElementById('discountPercentage').value) || 0,
                    taxRatePercentage: parseFloat(document.getElementById('taxRatePercentage').value) || 0,
                    paymentMethod: document.getElementById('paymentMethod').value,
                    notes: document.getElementById('notes').value,
                    items: []
                }
;


const itemRows = invoiceItemsContainer.querySelectorAll('.item-row');
if (itemRows.length === 0)
{
    document.getElementById('itemsError').textContent = 'Трябва да добавите поне един артикул.';
    messageArea.innerHTML = '<div class="alert alert-danger">Моля, коригирайте грешките във формата.</div>';
    return;
}

let isValid = true;
itemRows.forEach((row, index) => {
    const descriptionInput = row.querySelector(`[name = "Items[${index}].Description"]`);
    const quantityInput = row.querySelector(`[name = "Items[${index}].Quantity"]`);
    const unitPriceInput = row.querySelector(`[name = "Items[${index}].UnitPrice"]`);

    const item = {
                        description: descriptionInput.value,
                        quantity: parseFloat(quantityInput.value),
                        unitPrice: parseFloat(unitPriceInput.value)
                    };

if (!item.description.trim())
{
    document.getElementById(`itemDescription_${ index}
    Error`).textContent = 'Описанието е задължително.';
    isValid = false;
}
else if (item.description.length > 200)
{
    document.getElementById(`itemDescription_${ index}
    Error`).textContent = 'Описанието не може да надвишава 200 символа.';
    isValid = false;
}

if (isNaN(item.quantity) || item.quantity <= 0 || item.quantity > 1000000)
{
    document.getElementById(`itemQuantity_${ index}
    Error`).textContent = 'Количеството трябва да е положително число до 1 000 000.';
    isValid = false;
}

if (isNaN(item.unitPrice) || item.unitPrice <= 0 || item.unitPrice > 10000000)
{
    document.getElementById(`itemUnitPrice_${ index}
    Error`).textContent = 'Ед. цена трябва да е положително число до 10 000 000.';
    isValid = false;
}

requestData.items.push(item);
                });


if (!requestData.invoiceNumber.trim())
{
    document.getElementById('invoiceNumberError').textContent = 'Номерът на фактурата е задължителен.';
    isValid = false;
}
else if (requestData.invoiceNumber.length > 50)
{
    document.getElementById('invoiceNumberError').textContent = 'Номерът на фактурата не може да надвишава 50 символа.';
    isValid = false;
}

if (!requestData.issueDate)
{
    document.getElementById('issueDateError').textContent = 'Датата на издаване е задължителна.';
    isValid = false;
}

if (!requestData.sellerName.trim())
{
    document.getElementById('sellerNameError').textContent = 'Името на продавача е задължително.';
    isValid = false;
}
else if (requestData.sellerName.length > 200)
{
    document.getElementById('sellerNameError').textContent = 'Името на продавача не може да надвишава 200 символа.';
    isValid = false;
}

if (requestData.sellerAddress.length > 500)
{
    document.getElementById('sellerAddressError').textContent = 'Адресът на продавача не може да надвишава 500 символа.';
    isValid = false;
}
if (requestData.sellerEIK.length > 50)
{
    document.getElementById('sellerEIKError').textContent = 'ЕИК/БУЛСТАТ на продавача не може да надвишава 50 символа.';
    isValid = false;
}
if (requestData.sellerMOL.length > 50)
{
    document.getElementById('sellerMOLError').textContent = 'МОЛ на продавача не може да надвишава 50 символа.';
    isValid = false;
}

if (!requestData.buyerName.trim())
{
    document.getElementById('buyerNameError').textContent = 'Името на купувача е задължително.';
    isValid = false;
}
else if (requestData.buyerName.length > 200)
{
    document.getElementById('buyerNameError').textContent = 'Името на купувача не може да надвишава 200 символа.';
    isValid = false;
}

if (requestData.buyerAddress.length > 500)
{
    document.getElementById('buyerAddressError').textContent = 'Адресът на купувача не може да надвишава 500 символа.';
    isValid = false;
}
if (requestData.buyerEIK.length > 50)
{
    document.getElementById('buyerEIKError').textContent = 'ЕИК/БУЛСТАТ на купувача не може да надвишава 50 символа.';
    isValid = false;
}
if (requestData.buyerMOL.length > 50)
{
    document.getElementById('buyerMOLError').textContent = 'МОЛ на купувача не може да надвишава 50 символа.';
    isValid = false;
}

if (isNaN(requestData.discountPercentage) || requestData.discountPercentage < 0 || requestData.discountPercentage > 100)
{
    document.getElementById('discountPercentageError').textContent = 'Отстъпката трябва да е между 0 и 100.';
    isValid = false;
}
if (isNaN(requestData.taxRatePercentage) || requestData.taxRatePercentage < 0 || requestData.taxRatePercentage > 100)
{
    document.getElementById('taxRatePercentageError').textContent = 'Данъчната ставка трябва да е между 0 и 100.';
    isValid = false;
}

if (requestData.paymentMethod.length > 500)
{
    document.getElementById('paymentMethodError').textContent = 'Начинът на плащане не може да надвишава 500 символа.';
    isValid = false;
}
if (requestData.notes.length > 1000)
{
    document.getElementById('notesError').textContent = 'Бележките не могат да надвишават 1000 символа.';
    isValid = false;
}


if (!isValid)
{
    messageArea.innerHTML = '<div class="alert alert-danger">Моля, коригирайте грешките във формата.</div>';
    return;
}

try
{
    const response = await fetch('/api/InvoiceGenerator/generate', {
    method: 'POST',
                        headers:
        {
            'Content-Type': 'application/json',
                            'Accept': 'application/json'
                        },
                        body: JSON.stringify(requestData)
                    });

    if (response.ok)
    {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;

        const contentDisposition = response.headers.get('Content-Disposition');
        let filename = "generated_invoice.pdf";
        if (contentDisposition && contentDisposition.indexOf('filename=') !== -1)
        {
            const filenameMatch = / filename\*?= ['"]?(?:UTF-8'')?([^;"]+)/i.exec(contentDisposition);
                            if (filenameMatch && filenameMatch[1])
            {
                filename = decodeURIComponent(filenameMatch[1].replace(/\+/ g, ' '));
            }
        }

        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();

        messageArea.innerHTML = `< div class= "alert alert-success" >< i class= "fas fa-check-circle me-2" ></ i > Файлът на фактурата '${filename}' е успешно генериран и изтеглен!</div>`;
                    } else
{
    let errorText = await response.text();
    try
    {
        const errorJson = JSON.parse(errorText);
        if (errorJson.errors)
        {
           
            if (errorJson.errors["Items[0].Description"])
            { 
                document.getElementById('itemDescription_0Error').textContent = errorJson.errors["Items[0].Description"].join('; ');
            }
          
            if (errorJson.errors.InvoiceNumber)
            {
                document.getElementById('invoiceNumberError').textContent = errorJson.errors.InvoiceNumber.join('; ');
            }
            if (errorJson.errors.IssueDate)
            {
                document.getElementById('issueDateError').textContent = errorJson.errors.IssueDate.join('; ');
            }
            if (errorJson.errors.SellerName)
            {
                document.getElementById('sellerNameError').textContent = errorJson.errors.SellerName.join('; ');
            }
            if (errorJson.errors.SellerAddress)
            {
                document.getElementById('sellerAddressError').textContent = errorJson.errors.SellerAddress.join('; ');
            }
            if (errorJson.errors.SellerEIK)
            {
                document.getElementById('sellerEIKError').textContent = errorJson.errors.SellerEIK.join('; ');
            }
            if (errorJson.errors.SellerMOL)
            {
                document.getElementById('sellerMOLError').textContent = errorJson.errors.SellerMOL.join('; ');
            }
            if (errorJson.errors.BuyerName)
            {
                document.getElementById('buyerNameError').textContent = errorJson.errors.BuyerName.join('; ');
            }
            if (errorJson.errors.BuyerAddress)
            {
                document.getElementById('buyerAddressError').textContent = errorJson.errors.BuyerAddress.join('; ');
            }
            if (errorJson.errors.BuyerEIK)
            {
                document.getElementById('buyerEIKError').textContent = errorJson.errors.BuyerEIK.join('; ');
            }
            if (errorJson.errors.BuyerMOL)
            {
                document.getElementById('buyerMOLError').textContent = errorJson.errors.BuyerMOL.join('; ');
            }
            if (errorJson.errors.DiscountPercentage)
            {
                document.getElementById('discountPercentageError').textContent = errorJson.errors.DiscountPercentage.join('; ');
            }
            if (errorJson.errors.TaxRatePercentage)
            {
                document.getElementById('taxRatePercentageError').textContent = errorJson.errors.TaxRatePercentage.join('; ');
            }
            if (errorJson.errors.PaymentMethod)
            {
                document.getElementById('paymentMethodError').textContent = errorJson.errors.PaymentMethod.join('; ');
            }
            if (errorJson.errors.Notes)
            {
                document.getElementById('notesError').textContent = errorJson.errors.Notes.join('; ');
            }
            if (errorJson.errors.Items)
            {
                document.getElementById('itemsError').textContent = errorJson.errors.Items.join('; ');
            }
        }
        errorText = errorJson.message || response.statusText;
    }
    catch (e)
    {
        console.error("Failed to parse error response as JSON:", e);
    }
    messageArea.innerHTML = `< div class= "alert alert-danger" >< i class= "fas fa-times-circle me-2" ></ i > Грешка при генериране на фактура: ${ errorText}</ div >`;
                    }
                } catch (error) {
    console.error('Fetch error:', error);
    messageArea.innerHTML = `< div class= "alert alert-danger" >< i class= "fas fa-exclamation-triangle me-2" ></ i > Възникна мрежова грешка: ${ error.message}.</ div >`;
                }
            });
        });