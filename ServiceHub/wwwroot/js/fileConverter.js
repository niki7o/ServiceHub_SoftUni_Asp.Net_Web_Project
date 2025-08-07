document.getElementById('fileConvertForm').addEventListener('submit', async function (event) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);
    const messageBox = document.getElementById('messageBox');

    messageBox.classList.add('d-none');
    messageBox.classList.remove('bg-danger-light', 'text-danger', 'bg-success-light', 'text-success', 'bg-info-light', 'text-info');

    messageBox.textContent = 'Конвертирането започна... Моля, изчакайте.';
    messageBox.classList.remove('d-none');
    messageBox.classList.add('bg-info-light', 'text-info');

    try {
        const response = await fetch(form.action, {
            method: form.method,
            body: formData
        });

        if (response.ok) {
            const contentDisposition = response.headers.get('Content-Disposition');
            let filename = 'converted_file';
            if (contentDisposition && contentDisposition.indexOf('attachment') !== -1) {
                const filenameRegex = /filename\*?=['"]?(?:UTF-\d['']*)?([^;\n\r"']*)['']?;?/;
                const matches = filenameRegex.exec(contentDisposition);
                if (matches != null && matches[1]) {
                    filename = decodeURIComponent(matches[1].replace(/^utf-8'''/, ''));
                }
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);

            messageBox.textContent = 'Файлът е успешно конвертиран и изтеглен!';
            messageBox.classList.remove('d-none');
            messageBox.classList.add('bg-success-light', 'text-success');
        } else {
            const errorData = await response.json();
            messageBox.textContent = `Грешка: ${errorData.message || 'Неизвестна грешка.'}`;
            messageBox.classList.remove('d-none');
            messageBox.classList.add('bg-danger-light', 'text-danger');
        }
    } catch (error) {
        console.error('Fetch error:', error);
        messageBox.textContent = `Възникна мрежова грешка: ${error.message || 'Не може да се свърже със сървъра.'}`;
        messageBox.classList.remove('d-none');
        messageBox.classList.add('bg-danger-light', 'text-danger');
    }
});