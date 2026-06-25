let currentPage = 1;

function renderProductCard(product) {
    return `
        <div class="product-card">
            <a href="${product.url}" target="_blank">
                <img src="${product.imageUrl}" alt="${product.name}">
            </a>
            <p class="product-name">${product.name}</p>
            <p>Категория: ${product.category}</p>
            <p>Цена: ${product.price} руб.</p>
            <p class="old-price">Старая цена: ${product.oldPrice} руб.</p>
            <p class="discount">Скидка: ${product.discountPercent}</p>
            <p>Магазин: ${product.shop}</p>
            ${product.isBestDeal ? '<p class="best-deal">Лучшее предложение</p>' : ''}
        </div>
    `;
}

async function search(page = 1) {
    currentPage = page;
    const category = document.getElementById('category').value;
    const name = document.getElementById('name').value;
    const sortType = document.getElementById('sortType').value;

    let url = `${window.location.origin}/api/Products/Compare?page=${page}&sortingType=${sortType}`;
    if (category) url += `&category=${category}`;
    if (name) url += `&name=${name}`;

    const response = await fetch(url);
    const data = await response.json();
    displayResults(data);
}

async function displayFeatured() {
    document.getElementById('title').innerHTML = 'Хиты продаж';

    const url = `${window.location.origin}/api/Products/Main`;
    const response = await fetch(url);
    const data = await response.json();
    const featuredDiv = document.getElementById('featured');
    const resultsDiv = document.getElementById('results');
    const paginationDiv = document.getElementById('pagination');

    resultsDiv.innerHTML = '';
    paginationDiv.innerHTML = '';
    featuredDiv.innerHTML = '';

    data.forEach(product => {
        featuredDiv.innerHTML += renderProductCard(product);
    });
}

function displayResults(data) {
    const resultsDiv = document.getElementById('results');
    const featuredDiv = document.getElementById('featured');
    const titleDiv = document.getElementById('title').innerHTML = 'Результаты поиска';;
    featuredDiv.innerHTML = '';
    resultsDiv.innerHTML = '';

    data.items.forEach(product => {
        resultsDiv.innerHTML += renderProductCard(product);
    });

    renderPagination(data);
}

function renderPagination(data) {
    const paginationDiv = document.getElementById('pagination');
    const totalPages = data.totalPages;
    paginationDiv.innerHTML = '';

    paginationDiv.innerHTML += `
        <button onclick="search(1)" ${currentPage === 1 ? 'disabled' : ''}>Первая</button>
        <button onclick="search(${currentPage - 1})" ${currentPage === 1 ? 'disabled' : ''}>Назад</button>
        <span>Страница ${currentPage} из ${totalPages}</span>
        <button onclick="search(${currentPage + 1})" ${currentPage === totalPages ? 'disabled' : ''}>Вперёд</button>
        <button onclick="search(${totalPages})" ${currentPage === totalPages ? 'disabled' : ''}>Последняя</button>
        <input type="number" id="jumpPage" min="1" max="${totalPages}" placeholder="№ страницы">
        <button onclick="jumpToPage(${totalPages})">Перейти</button>
    `;
}

function jumpToPage(totalPages) {
    const input = document.getElementById('jumpPage');
    let page = parseInt(input.value);
    if (!page || page < 1) page = 1;
    if (page > totalPages) page = totalPages;
    search(page);
}
displayFeatured();