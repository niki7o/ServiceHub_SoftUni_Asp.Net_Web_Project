
const isBusinessUser = @(isBusinessUserOrAdmin.ToString().ToLower());
const supportedLanguages = @Html.Raw(Json.Serialize(supportedLanguages));
const lockedLanguages = new Set(['JavaScript', 'PHP']);
const subscriptionPageUrl = "@subscriptionPageUrl";

function showConversionMessage(message, type) {
    const conversionMessage = document.getElementById('conversionMessage');
    conversionMessage.textContent = message;
    conversionMessage.className = `mt-2 p-3 rounded alert alert-${type}`;
    conversionMessage.style.display = 'block';
}

function handlePremiumMessage(selectElement, premiumMessageElement) {
    const selectedLanguage = selectElement.value;

    if (lockedLanguages.has(selectedLanguage) && !isBusinessUser) {
        premiumMessageElement.style.display = 'flex';
        showConversionMessage('Моля, надстройте акаунта си, за да използвате този език.', 'warning');
    } else {
        premiumMessageElement.style.display = 'none';

        if (document.getElementById('conversionMessage').classList.contains('alert-warning')) {
            showConversionMessage('', 'none');
        }
    }
}

function populateLanguageSelect(selectElement) {

    selectElement.innerHTML = '';

    const defaultOption = document.createElement('option');
    defaultOption.value = "";
    defaultOption.textContent = "-- Изберете език --";
    selectElement.appendChild(defaultOption);

    supportedLanguages.forEach(lang => {
        const option = document.createElement('option');
        option.value = lang;
        option.textContent = lang;

        const isLocked = lockedLanguages.has(lang);
        option.dataset.isLocked = isLocked.toString();


        if (isLocked && !isBusinessUser) {
            option.disabled = true;
            option.textContent += ' (ПРЕМИУМ) \u{1F512}';
        }
        selectElement.appendChild(option);
    });
}

document.addEventListener('DOMContentLoaded', () => {
    const sourceLanguageSelect = document.getElementById('sourceLanguage');
    const targetLanguageSelect = document.getElementById('targetLanguage');
    const sourceLanguagePremiumMessage = document.getElementById('sourceLanguagePremiumMessage');
    const targetLanguagePremiumMessage = document.getElementById('targetLanguagePremiumMessage');

    populateLanguageSelect(sourceLanguageSelect);
    populateLanguageSelect(targetLanguageSelect);

    sourceLanguageSelect.addEventListener('change', () => {
        handlePremiumMessage(sourceLanguageSelect, sourceLanguagePremiumMessage);
    });
    targetLanguageSelect.addEventListener('change', () => {
        handlePremiumMessage(targetLanguageSelect, targetLanguagePremiumMessage);
    });


    handlePremiumMessage(sourceLanguageSelect, sourceLanguagePremiumMessage);
    handlePremiumMessage(targetLanguageSelect, targetLanguagePremiumMessage);
});

document.getElementById('convertCodeButton').addEventListener('click', async () => {
    const sourceCode = document.getElementById('sourceCode').value;
    const sourceLanguage = document.getElementById('sourceLanguage').value;
    const targetLanguage = document.getElementById('targetLanguage').value;
    const convertedCodeOutput = document.getElementById('convertedCode');

    convertedCodeOutput.value = '';
    showConversionMessage('', 'none');

    if (!sourceCode.trim()) {
        showConversionMessage('Моля, въведете изходен код.', 'danger');
        return;
    }
    if (!sourceLanguage) {
        showConversionMessage('Моля, изберете изходен език.', 'danger');
        return;
    }
    if (!targetLanguage) {
        showConversionMessage('Моля, изберете целеви език.', 'danger');
        return;
    }
    if (sourceLanguage === targetLanguage) {
        showConversionMessage('Изходният и целевият език не могат да бъдат еднакви. Върнат е оригиналният код.', 'warning');
        convertedCodeOutput.value = sourceCode;
        return;
    }


    if (!isBusinessUser && (lockedLanguages.has(sourceLanguage) || lockedLanguages.has(targetLanguage))) {
        showConversionMessage('Достъпът до JavaScript и PHP конвертиране е само за Бизнес Потребители. Моля, надстройте акаунта си.', 'warning');
        return;
    }

    try {
        showConversionMessage('Конвертиране на кода... Моля, изчакайте.', 'info');
        document.getElementById('convertCodeButton').disabled = true;

        const response = await fetch('/api/CodeSnippetConverter/convert', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                sourceCode: sourceCode,
                sourceLanguage: sourceLanguage,
                targetLanguage: targetLanguage
            })
        });

        if (response.ok) {
            const result = await response.json();
            convertedCodeOutput.value = result.convertedCode;
            showConversionMessage(result.message || 'Конвертирането е успешно.', 'success');
        } else {
            let errorMessage = `Грешка: ${response.status} ${response.statusText}`;
            try {
                const errorData = await response.json();
                errorMessage = errorData.errors ? Object.values(errorData.errors).flat().join('; ') : (errorData.message || errorData.title || errorMessage);
            } catch (jsonError) {
                errorMessage = `Грешка: ${response.status} ${response.statusText}. Невалиден отговор от сървъра.`;
                console.error('Failed to parse error response as JSON:', jsonError);
            }
            convertedCodeOutput.value = '';
            showConversionMessage(errorMessage, 'danger');
        }
    } catch (error) {
        console.error('Fetch error:', error);
        convertedCodeOutput.value = '';
        showConversionMessage(`Възникна мрежова грешка: ${error.message}.`, 'danger');
    } finally {
        document.getElementById('convertCodeButton').disabled = false;
    }
});