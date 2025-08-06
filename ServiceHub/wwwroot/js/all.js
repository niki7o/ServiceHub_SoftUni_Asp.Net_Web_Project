document.addEventListener('DOMContentLoaded', function () {

    const categoryFilter = document.getElementById('categoryFilter');
    const accessTypeFilter = document.getElementById('accessTypeFilter');
    const sortFilter = document.getElementById('sort');
    const filterDropdown = document.getElementById('filter');
    const searchInput = document.getElementById('searchInput');
    const searchResultsDiv = document.getElementById('searchResults');
    const noResultsMessageElement = searchResultsDiv.querySelector('.no-results');
    const servicesListContainer = document.getElementById('servicesListContainer');
    const allServiceCards = servicesListContainer.querySelectorAll('.service-card-wrapper');
    const initialNoServicesMessage = servicesListContainer.querySelector('.no-services-message');
    const paginationContainer = document.querySelector('.pagination-container');

    let searchTimeout;
    const transitionDuration = 300;

    function applySingleCardStyles() {
        const visibleCards = Array.from(allServiceCards).filter(card => card.style.display !== 'none' && !card.classList.contains('hide'));

        allServiceCards.forEach(card => card.classList.remove('single-visible-card'));

        if (visibleCards.length === 1) {
            visibleCards[0].classList.add('single-visible-card');
        }
    }

   
    const currentCategory = window.AppConfig.currentCategory;
    if (categoryFilter) {
        categoryFilter.value = currentCategory;
    }

    const currentAccessType = window.AppConfig.currentAccessType;
    if (accessTypeFilter) {
        accessTypeFilter.value = currentAccessType;
    }

    const currentSort = window.AppConfig.currentSort;
    if (sortFilter) {
        sortFilter.value = currentSort;
    }

    const currentFilterValue = window.AppConfig.currentFilter;
    if (filterDropdown) {
        filterDropdown.value = currentFilterValue;
    }

    // Използвайте глобалните променливи за TempData съобщенията
    const successMessageExists = window.AppMessages.hasSuccessMessage;
    const errorMessageExists = window.AppMessages.hasErrorMessage;

    if (successMessageExists || errorMessageExists) {
        if (searchInput) {
            searchInput.value = '';

            allServiceCards.forEach(card => {
                card.style.display = '';
                card.classList.remove('hide');
            });
            noResultsMessageElement.textContent = '';
            searchResultsDiv.classList.remove('show');
            searchResultsDiv.style.display = 'none';

            if (initialNoServicesMessage) {
                initialNoServicesMessage.classList.add('hide');
                initialNoServicesMessage.style.display = 'none';
            }

            if (paginationContainer) {
                paginationContainer.style.display = '';
                paginationContainer.classList.remove('hide');
            }
            applySingleCardStyles();
        }
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/searchHub")
        .build();

    connection.on("ReceiveSearchResults", (services) => {
        const foundServiceIds = new Set(services.map(s => s.id));
        let foundCount = 0;

        allServiceCards.forEach(card => {
            const serviceId = card.dataset.serviceId;
            if (foundServiceIds.has(serviceId)) {
                card.style.display = '';
                setTimeout(() => {
                    card.classList.remove('hide');
                }, 10);
                foundCount++;
            } else {
                card.classList.add('hide');
                setTimeout(() => {
                    card.style.display = 'none';
                }, transitionDuration);
            }
        });

        setTimeout(applySingleCardStyles, transitionDuration + 50);

        if (foundCount === 0 && searchInput.value.trim().length > 0) {
            noResultsMessageElement.textContent = 'Няма намерени услуги.';
            searchResultsDiv.classList.add('show');
            searchResultsDiv.style.display = 'block';
            if (initialNoServicesMessage) initialNoServicesMessage.classList.add('hide');
            if (paginationContainer) {
                paginationContainer.classList.add('hide');
                setTimeout(() => { paginationContainer.style.display = 'none'; }, transitionDuration);
            }
        } else {
            noResultsMessageElement.textContent = '';
            searchResultsDiv.classList.remove('show');
            setTimeout(() => { searchResultsDiv.style.display = 'none'; }, transitionDuration);

            if (initialNoServicesMessage && window.AppConfig.modelServicesLength === 0 && searchInput.value.trim().length === 0) {
                initialNoServicesMessage.classList.remove('hide');
                initialNoServicesMessage.style.display = 'block';
            } else if (initialNoServicesMessage) {
                initialNoServicesMessage.classList.add('hide');
                setTimeout(() => { initialNoServicesMessage.style.display = 'none'; }, transitionDuration);
            }

            if (paginationContainer) {
                paginationContainer.style.display = '';
                setTimeout(() => { paginationContainer.classList.remove('hide'); }, 10);
            }
            applySingleCardStyles();
        }
    });

    connection.start().catch(err => console.error(err.toString()));

    searchInput.addEventListener('keyup', () => {
        clearTimeout(searchTimeout);
        const searchTerm = searchInput.value.trim();

        if (searchTerm.length > 2) {
            searchTimeout = setTimeout(() => {
                connection.invoke("SearchServices", searchTerm).catch(err => console.error(err.toString()));
            }, 300);
        } else if (searchTerm.length === 0) {
            allServiceCards.forEach(card => {
                card.style.display = '';
                setTimeout(() => { card.classList.remove('hide'); }, 10);
            });
            noResultsMessageElement.textContent = '';
            searchResultsDiv.classList.remove('show');
            setTimeout(() => { searchResultsDiv.style.display = 'none'; }, transitionDuration);

            if (initialNoServicesMessage && window.AppConfig.modelServicesLength === 0) {
                initialNoServicesMessage.classList.remove('hide');
                initialNoServicesMessage.style.display = 'block';
            } else if (initialNoServicesMessage) {
                initialNoServicesMessage.classList.add('hide');
                setTimeout(() => { initialNoServicesMessage.style.display = 'none'; }, transitionDuration);
            }

            if (paginationContainer) {
                paginationContainer.style.display = '';
                setTimeout(() => { paginationContainer.classList.remove('hide'); }, 10);
            }
            applySingleCardStyles();
        } else {
            allServiceCards.forEach(card => {
                card.style.display = '';
                setTimeout(() => { card.classList.remove('hide'); }, 10);
            });
            noResultsMessageElement.textContent = '';
            searchResultsDiv.classList.remove('show');
            setTimeout(() => { searchResultsDiv.style.display = 'none'; }, transitionDuration);

            if (initialNoServicesMessage) {
                initialNoServicesMessage.classList.add('hide');
                setTimeout(() => { initialNoServicesMessage.style.display = 'none'; }, transitionDuration);
            }

            if (paginationContainer) {
                paginationContainer.style.display = '';
                setTimeout(() => { paginationContainer.classList.remove('hide'); }, 10);
            }
            applySingleCardStyles();
        }
    });

    searchInput.addEventListener('focus', () => {
        const searchTerm = searchInput.value.trim();
        if (searchTerm.length > 0) {
            connection.invoke("SearchServices", searchTerm).catch(err => console.error(err.toString()));
        } else {
            allServiceCards.forEach(card => {
                card.style.display = '';
                setTimeout(() => { card.classList.remove('hide'); }, 10);
            });
            noResultsMessageElement.textContent = '';
            searchResultsDiv.classList.remove('show');
            setTimeout(() => { searchResultsDiv.style.display = 'none'; }, transitionDuration);

            if (initialNoServicesMessage && window.AppConfig.modelServicesLength === 0) {
                initialNoServicesMessage.classList.remove('hide');
                initialNoServicesMessage.style.display = 'block';
            } else if (initialNoServicesMessage) {
                initialNoServicesMessage.classList.add('hide');
                setTimeout(() => { initialNoServicesMessage.style.display = 'none'; }, transitionDuration);
            }

            if (paginationContainer) {
                paginationContainer.style.display = '';
                setTimeout(() => { paginationContainer.classList.remove('hide'); }, 10);
            }
        }
    });

    document.addEventListener('click', function (event) {
        if (!searchResultsDiv.contains(event.target) && event.target !== searchInput) {
            searchResultsDiv.classList.remove('show');
            setTimeout(() => { searchResultsDiv.style.display = 'none'; }, transitionDuration);
        }
    });
});
