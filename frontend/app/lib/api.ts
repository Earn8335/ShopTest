const API_URL = "http://localhost:5000/api";

export type Product = {
  productId: number;
  productCode: string;
  productName: string;
  unitPrice: number;
  description: string | null;
  stockQuantity: number;
};

export type CartItem = {
  productId: number;
  productCode: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
};

export type Cart = { cartId: number; items: CartItem[]; totalAmount: number };
export type Order = { orderId: number; totalAmount: number; items: CartItem[] };

async function request<T>(
  path: string,
  method = "GET",
  body?: unknown,
): Promise<T> {
  const response = await fetch(API_URL + path, {
    method,
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    const error = await response.json().catch(() => null);
    throw new Error(error?.message ?? `เกิดข้อผิดพลาด (${response.status})`);
  }
  return response.json();
}

export const getProducts = () => request<Product[]>("/products");
export const getCart = () => request<Cart>("/cart");
export const addToCart = (productId: number) =>
  request<Cart>("/cart/items", "POST", { productId, quantity: 1 });
export const setQuantity = (productId: number, quantity: number) =>
  request<Cart>(`/cart/items/${productId}`, "PUT", { quantity });
export const removeItem = (productId: number) =>
  request<Cart>(`/cart/items/${productId}`, "DELETE");
export const clearCart = () => request<Cart>("/cart", "DELETE");
export const checkout = () => request<Order>("/checkout", "POST");
